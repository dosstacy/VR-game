using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class CablePortSlot
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    public CableId expectedCable = CableId.None;
}

[Serializable]
public class CableCombinationEffect
{
    [Header("Combination")]
    [Tooltip("Name of this combination (for debugging)")]
    public string combinationName = "Wrong Combination";
    
    [Tooltip("Expected cable connections. Order must match slots array!")]
    public CableId[] expectedCables;
    
    [Header("Effects")]
    [Tooltip("What happens when this combination is detected")]
    public CombinationEffectType effectType = CombinationEffectType.WrongCombination;
    
    [Tooltip("Audio to play")]
    public AudioClip[] audioClips;

    [Tooltip("Objects that will have their materials changed")]
    public MaterialChangeConfig[] materialsToChange;
    
    [Tooltip("Custom message to display")]
    public string displayMessage = "";
    public TMPro.TextMeshProUGUI textMesh;
    
    [Header("Quest Completion")]
    [Tooltip("Does this combination complete the quest?")]
    public bool completesQuest = false;
    
    [Header("Party Mode (Optional)")]
    [Tooltip("Enable color cycling animation when this combination is active")]
    public bool enablePartyMode = false;
    
    [Tooltip("Colors to cycle through in party mode")]
    public Color[] partyColors;
    
    [Tooltip("How long each color is shown (in seconds)")]
    public float colorCycleDuration = 2f;
}

[Serializable]
public class MaterialChangeConfig
{
    [Tooltip("Object whose material will be changed (e.g., molding_wall, plinth_wall)")]
    public Renderer targetRenderer;
    
    [Tooltip("Material to apply")]
    public Material materialToApply;
    
    [Tooltip("Optional: Specific material index to change (leave -1 for all materials)")]
    public int materialIndex = -1;
}

public enum CombinationEffectType
{
    WrongCombination,  
    CorrectCombination 
}

public class ServerRoomQuest : MonoBehaviour
{
    [Header("Ports")]
    [SerializeField] private CablePortSlot[] slots;

    [Header("Cable Combinations")]
    [Tooltip("All possible cable combinations and their effects")]
    [SerializeField] private CableCombinationEffect[] combinations;

    [Header("Quest")]
    [SerializeField] private Door.QuestId questToComplete = Door.QuestId.ServerRoom;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugMessages = true;

    [Header("Audio to Stop on Completion")]
    [Tooltip("AudioSource that will stop playing after quest completion")]
    [SerializeField] private AudioSource audioToStop;

    [Tooltip("Fade out audio smoothly")]
    [SerializeField] private bool fadeOutAudio = true;

    [Tooltip("Duration of fade out")]
    [SerializeField] private float audioFadeOutDuration = 2f;

    private bool isAudioFading = false;
    private float audioFadeTimer = 0f;
    private float audioOriginalVolume = 1f;

    private bool completed;
    private CableCombinationEffect currentCombination;
    private Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor, CablePortSlot> socketToSlotMap;
    
    // Store original materials for restoration
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    
    // Party mode coroutine reference
    private Coroutine partyModeCoroutine;
    private Coroutine messageCoroutine;
    private readonly List<AudioSource> activeAudioSources = new();

    private void Awake()
    {
        ValidateSetup();
        InitializeSocketMap();
        SaveOriginalMaterials();
        
        // Create audio source if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }

        if (audioToStop != null)
        {
            audioOriginalVolume = audioToStop.volume;
        }
    }

    private void Update()
    {
        // Fade out logic
        if (isAudioFading && audioToStop != null)
        {
            audioFadeTimer += Time.deltaTime;
            float t = audioFadeTimer / audioFadeOutDuration;
            audioToStop.volume = Mathf.Lerp(audioOriginalVolume, 0f, t);
            
            if (t >= 1f)
            {
                audioToStop.Stop();
                isAudioFading = false;
            }
        }
    }

    private void ValidateSetup()
    {
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("Slots are not set!", this);
            return;
        }

        foreach (var s in slots)
        {
            if (s.socket == null)
                Debug.LogError("One of slots has no socket assigned!", this);

            if (s.expectedCable == CableId.None)
                Debug.LogWarning("One of slots has expectedCable=None! This might be intentional.", this);
        }
        
        if (combinations == null || combinations.Length == 0)
        {
            Debug.LogError("No combinations defined!", this);
            return;
        }
        
        // Validate each combination
        foreach (var combo in combinations)
        {
            if (combo.expectedCables == null || combo.expectedCables.Length != slots.Length)
            {
                Debug.LogError($"Combination '{combo.combinationName}' has wrong number of cables! Expected {slots.Length}, got {combo.expectedCables?.Length ?? 0}", this);
            }
        }
    }

    private void InitializeSocketMap()
    {
        socketToSlotMap = new Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor, CablePortSlot>();
        
        foreach (var slot in slots)
        {
            if (slot.socket != null)
            {
                socketToSlotMap[slot.socket] = slot;
            }
        }
    }

    private void OnEnable()
    {
        if (slots == null) return;

        foreach (var s in slots)
        {
            if (s.socket == null) continue;
            s.socket.selectEntered.AddListener(OnSocketChanged);
            s.socket.selectExited.AddListener(OnSocketChanged);
        }
    }

    private void OnDisable()
    {
        if (slots == null) return;

        foreach (var s in slots)
        {
            if (s.socket == null) continue;
            s.socket.selectEntered.RemoveListener(OnSocketChanged);
            s.socket.selectExited.RemoveListener(OnSocketChanged);
        }
    }

    private void OnSocketChanged(BaseInteractionEventArgs args)
    {
        if (completed) return;

        // Check if we have a complete combination
        if (AllSocketsFilled())
        {
            CheckCombination();
        }
        else
        {
            // Reset effects if not all sockets are filled
            ResetEffects();
        }
    }

    private bool AllSocketsFilled()
    {
        return slots.All(slot => slot.socket != null && slot.socket.hasSelection);
    }

    private void CheckCombination()
    {
        // Get current cable configuration
        CableId[] currentCables = new CableId[slots.Length];
        
        for (int i = 0; i < slots.Length; i++)
        {
            var cable = GetCableFromSocket(slots[i].socket);
            currentCables[i] = cable != null ? cable.id : CableId.None;
        }

        // Find matching combination
        CableCombinationEffect matchedCombination = null;
        
        foreach (var combo in combinations)
        {
            if (CombinationMatches(currentCables, combo.expectedCables))
            {
                matchedCombination = combo;
                break;
            }
        }

        if (matchedCombination != null)
        {
            if (showDebugMessages)
                Debug.Log($"Combination detected: {matchedCombination.combinationName}");
            
            ApplyCombinationEffects(matchedCombination);
            currentCombination = matchedCombination;
        }
        else
        {
            if (showDebugMessages)
                Debug.Log("Unknown combination - no effects");
            
            ResetEffects();
        }
    }

    private bool CombinationMatches(CableId[] current, CableId[] expected)
    {
        Debug.Log("Checking combination: " + string.Join(",", current) + " vs " + string.Join(",", expected));
        if (current.Length != expected.Length)
            return false;

        for (int i = 0; i < current.Length; i++)
        {
            if (current[i] != expected[i])
                return false;
        }

        return true;
    }

    private void ApplyCombinationEffects(CableCombinationEffect combo)
    {
        // Reset previous effects first
        ResetEffects();

        // Play audio
        if (combo.audioClips != null)
        {
            foreach (var clip in combo.audioClips)
            {
                if (clip == null) continue;

                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.clip = clip;
                src.Play();

                activeAudioSources.Add(src);
            }
        }

        // Change materials
        if (combo.materialsToChange != null)
        {
            foreach (var matConfig in combo.materialsToChange)
            {
                if (matConfig.targetRenderer != null && matConfig.materialToApply != null)
                {
                    ChangeMaterial(matConfig);
                }
            }
        }

        // Display message
        if (!string.IsNullOrEmpty(combo.displayMessage))
        {
            if (messageCoroutine != null)
                StopCoroutine(messageCoroutine);

            messageCoroutine = StartCoroutine(ShowMessage(combo, 3f));
        }

        // Start party mode if enabled
        if (combo.enablePartyMode && combo.partyColors != null && combo.partyColors.Length > 0)
        {
            StartPartyMode(combo);
        }

        // Complete quest if this is the correct combination
        if (combo.completesQuest && !completed)
        {
            completed = true;
            
            if (showDebugMessages)
                Debug.Log("🎉 Quest completed!");

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.CompleteQuest(questToComplete);

                if (audioToStop != null)
                {
                    if (fadeOutAudio)
                    {
                        isAudioFading = true;
                        audioFadeTimer = 0f;
                    }
                    else
                    {
                        audioToStop.Stop();
                    }
                    Debug.Log("Stopping audio after quest completion.");
                }

                var roomMusic = FindObjectOfType<RoomMusicController>();
                if (roomMusic != null)
                {
                    roomMusic.ActivateMusic();
                }
            }
            else
            {
                Debug.LogWarning("QuestManager.Instance is null; cannot complete quest.", this);
            }
        }
    }

    private void ResetEffects()
    {
        // Stop audio
        foreach (var src in activeAudioSources)
        {
            if (src != null)
            {
                src.Stop();
                Destroy(src);
            }
        }
        activeAudioSources.Clear();
        

        // Stop party mode
        StopPartyMode();

        // Restore original materials
        RestoreOriginalMaterials();

        currentCombination = null;
    }

    private void SaveOriginalMaterials()
    {
        if (combinations == null) return;

        originalMaterials.Clear();

        // Collect all renderers that will be changed
        foreach (var combo in combinations)
        {
            if (combo.materialsToChange == null) continue;

            foreach (var matConfig in combo.materialsToChange)
            {
                if (matConfig.targetRenderer != null && !originalMaterials.ContainsKey(matConfig.targetRenderer))
                {
                    // Save all materials from this renderer
                    Material[] mats = matConfig.targetRenderer.materials;
                    originalMaterials[matConfig.targetRenderer] = mats;
                    
                    if (showDebugMessages)
                        Debug.Log($"Saved original materials for: {matConfig.targetRenderer.name}");
                }
            }
        }
    }

    private void ChangeMaterial(MaterialChangeConfig config)
    {
        if (config.targetRenderer == null || config.materialToApply == null)
            return;

        if (config.materialIndex >= 0)
        {
            // Change specific material slot
            Material[] mats = config.targetRenderer.materials;
            if (config.materialIndex < mats.Length)
            {
                mats[config.materialIndex] = config.materialToApply;
                config.targetRenderer.materials = mats;
                
                if (showDebugMessages)
                    Debug.Log($"Changed material [{config.materialIndex}] on {config.targetRenderer.name}");
            }
        }
        else
        {
            // Change all materials
            Material[] mats = new Material[config.targetRenderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = config.materialToApply;
            }
            config.targetRenderer.materials = mats;
            
            if (showDebugMessages)
                Debug.Log($"Changed all materials on {config.targetRenderer.name}");
        }
    }

    private void RestoreOriginalMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.materials = kvp.Value;
                
                if (showDebugMessages)
                    Debug.Log($"Restored materials for: {kvp.Key.name}");
            }
        }
    }

    private Cable GetCableFromSocket(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket)
    {
        if (socket == null || !socket.hasSelection) return null;

        var interactable = socket.interactablesSelected.FirstOrDefault();
        if (interactable == null) return null;

        return interactable.transform.GetComponent<Cable>();
    }

    // Party mode implementation
    private void StartPartyMode(CableCombinationEffect combo)
    {
        StopPartyMode(); // Stop any existing party mode
        
        if (combo.materialsToChange != null && combo.materialsToChange.Length > 0)
        {
            partyModeCoroutine = StartCoroutine(PartyModeCoroutine(combo));
            
            if (showDebugMessages)
                Debug.Log("🎉 Party mode started!");
        }
    }

    private void StopPartyMode()
    {
        if (partyModeCoroutine != null)
        {
            StopCoroutine(partyModeCoroutine);
            partyModeCoroutine = null;
            
            if (showDebugMessages)
                Debug.Log("Party mode stopped");
        }
    }

    private System.Collections.IEnumerator PartyModeCoroutine(CableCombinationEffect combo)
    {
        int colorIndex = 0;
        
        while (true)
        {
            Debug.Log("Party mode cycle: " + colorIndex);
            // Get current color
            Color currentColor = combo.partyColors[colorIndex];
            
            // Apply color to all materials
            foreach (var matConfig in combo.materialsToChange)
            {
                if (matConfig.targetRenderer != null)
                {
                    ApplyEmissionColor(matConfig.targetRenderer, currentColor);
                }
            }
            
            if (showDebugMessages)
                Debug.Log($"Party mode: Changed to color {colorIndex} ({currentColor})");
            
            // Wait for duration
            yield return new WaitForSeconds(combo.colorCycleDuration);
            
            // Move to next color
            colorIndex = (colorIndex + 1) % combo.partyColors.Length;
        }
    }

    private System.Collections.IEnumerator ShowMessage(CableCombinationEffect combo, float duration)
    {
        if (combo.textMesh == null) yield break;

        combo.textMesh.text = combo.displayMessage;
        combo.textMesh.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        combo.textMesh.gameObject.SetActive(false);
    }

    private void ApplyEmissionColor(Renderer renderer, Color color)
    {
        // Use MaterialPropertyBlock to avoid creating material instances
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        
        if (renderer.materials.Length > 0)
        {
            for (int i = 0; i < renderer.materials.Length; i++)
            {
                renderer.GetPropertyBlock(block, i);
                
                // Set emission color (works with Standard and URP shaders)
                block.SetColor("_EmissionColor", color);
                
                // Also set base color if needed
                block.SetColor("_Color", color);
                block.SetColor("_BaseColor", color); // For URP
                
                renderer.SetPropertyBlock(block, i);
            }
        }
    }
}