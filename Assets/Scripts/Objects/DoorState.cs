using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Door component that can be locked or unlocked based on quests and/or keys.
/// </summary>
public class Door : MonoBehaviour
{
    public enum QuestId { None, ChildRoom, ServerRoom, Maze, FinalRoom }
    public enum DoorState { Locked, Open }
    public enum UnlockType { Quest, Key, QuestAndKey, None }

    [Header("State")]
    public DoorState state = DoorState.Locked;
    
    [Header("Unlock Requirements")]
    [Tooltip("How this door can be unlocked")]
    public UnlockType unlockType = UnlockType.Quest;
    
    [Tooltip("Required quest to unlock (if unlockType = Quest)")]
    public QuestId requiredQuest = QuestId.None;
    
    [Tooltip("Required keys to unlock (if unlockType = Key)")]
    public KeyType[] acceptedKeys;

    [Header("Renderers")]
    [SerializeField] private MeshRenderer frameRenderer;
    [SerializeField] private MeshRenderer doorRenderer;
    [SerializeField] private MeshRenderer handleRenderer;

    [Header("Animation")]
    [SerializeField] private Animator doorAnimator;

    [Header("Door frame materials")]
    [SerializeField] private Material lockedDoorFrameMaterial;
    [SerializeField] private Material OpenDoorFrameMaterial;

    [Header("Door materials")]
    [SerializeField] private Material lockedDoorMaterial;
    [SerializeField] private Material OpenDoorMaterial;

    [Header("Door handle materials")]
    [SerializeField] private Material lockedHandleMaterial;
    [SerializeField] private Material OpenHandleMaterial;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorOpenAudio;

    [Header("Messages")]
    [SerializeField] private QuestText questText;
    [SerializeField] private string doorOpenedMessage = "The doors are opened with keys.";
    [SerializeField] private string noKeyMessage = "Doors are locked. Keys may be in one of the rooms...";
    [SerializeField] private string questRequiredMessage = "Doors are locked. You need to complete a quest.";
    private bool wasUnlockedByKey = false;

    private void Awake()
    {
        ValidateComponents();
    }

    private void Start()
    {
        Debug.Log($"Door '{name}' initialized in state: {state} (unlock type: {unlockType})");
        UpdateVisuals();
        
        // Check if door should be auto-unlocked from save
        CheckAutoUnlock();
    }

    private void ValidateComponents()
    {
        if (doorRenderer == null || frameRenderer == null || handleRenderer == null)
            Debug.LogError("One or more door renderers are not assigned!", this);

        if (doorAnimator == null)
            Debug.LogWarning("Door Animator is not assigned!", this);

        if (lockedDoorMaterial == null || OpenDoorMaterial == null)
            Debug.LogError("One or more door materials are not assigned!", this);

        if (lockedHandleMaterial == null || OpenHandleMaterial == null)
            Debug.LogError("One or more door handle materials are not assigned!", this);

        if (lockedDoorFrameMaterial == null || OpenDoorFrameMaterial == null)
            Debug.LogError("One or more door frame materials are not assigned!", this);

        // Validation based on unlock type
        if (unlockType == UnlockType.Quest && requiredQuest == QuestId.None && state == DoorState.Locked)
        {
            Debug.LogWarning("Door unlock type is Quest but no required quest is set!", this);
        }

        if (unlockType == UnlockType.Key && (acceptedKeys == null || acceptedKeys.Length == 0))
        {
            Debug.LogWarning("Door unlock type is Key but no accepted keys are set!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        HandlePlayerApproach();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        TryCloseDoor();
    }

    private void HandlePlayerApproach()
    {
        // if the doors are already open - just play the opening animation
        if (state == DoorState.Open)
        {
            TryOpenDoor();
            return;
        }

        // Doors are locked - check conditions
        switch (unlockType)
        {
            case UnlockType.Key:
                if (HasRequiredKey())
                {
                    UnlockWithKey();
                }
                else
                {
                    ShowNoKeyMessage();
                }
                break;

            case UnlockType.Quest:
                ShowQuestRequiredMessage();
                break;

            case UnlockType.QuestAndKey:
                HandleQuestAndKeyDoor();
                break;

            case UnlockType.None:
                break;
        }
    }

    private bool HasRequiredKey()
    {
        if (KeyManager.Instance == null) return false;
        if (acceptedKeys == null || acceptedKeys.Length == 0) return false;

        foreach (var keyType in acceptedKeys)
        {
            if (KeyManager.Instance.HasKey(keyType))
            {
                return true;
            }
        }

        return false;
    }

    private void UnlockWithKey()
    {
        if (wasUnlockedByKey) return; // Already unlocked

        wasUnlockedByKey = true;
        Unlock();

        // Play audio
        if (audioSource != null && doorOpenAudio != null)
        {
            audioSource.PlayOneShot(doorOpenAudio);
        }

        // Show message
        EnsureQuestText();
        if (questText != null && !string.IsNullOrEmpty(doorOpenedMessage))
        {
            questText.Show(doorOpenedMessage);
        }

        // Open door
        TryOpenDoor();

        Debug.Log($"Door '{name}' unlocked with key!");
    }

    private void ShowNoKeyMessage()
    {
        EnsureQuestText();
        if (questText != null && !string.IsNullOrEmpty(noKeyMessage))
        {
            questText.Show(noKeyMessage);
        }

        Debug.Log($"Door '{name}' is locked. Player needs a key.");
    }

    private void ShowQuestRequiredMessage()
    {
        EnsureQuestText();
        if (questText != null && !string.IsNullOrEmpty(questRequiredMessage))
        {
            questText.Show(questRequiredMessage);
        }

        Debug.Log($"Door '{name}' requires quest '{requiredQuest}' to be completed.");
    }

    private void CheckAutoUnlock()
    {
        // For key-based doors, check if player already has the key
        if (unlockType == UnlockType.Key && HasRequiredKey() && state == DoorState.Locked)
        {
            Unlock();
            Debug.Log($"Door '{name}' auto-unlocked (player already has key).");
        }
    }

    private void UpdateVisuals()
    {
        if (doorRenderer == null) return;

        switch (state)
        {
            case DoorState.Locked:
                if (lockedDoorMaterial != null) doorRenderer.sharedMaterial = lockedDoorMaterial;
                if (lockedHandleMaterial != null) handleRenderer.sharedMaterial = lockedHandleMaterial;
                if (lockedDoorFrameMaterial != null) frameRenderer.sharedMaterial = lockedDoorFrameMaterial;
                break;
            case DoorState.Open:
                if (OpenDoorMaterial != null) doorRenderer.sharedMaterial = OpenDoorMaterial;
                if (OpenHandleMaterial != null) handleRenderer.sharedMaterial = OpenHandleMaterial;
                if (OpenDoorFrameMaterial != null) frameRenderer.sharedMaterial = OpenDoorFrameMaterial;
                break;
        }
    }

    // ensure we have a questText reference (tries a global lookup)
    private void EnsureQuestText()
    {
        if (questText == null)
        {
            questText = FindObjectOfType<QuestText>();
            if (questText != null)
                Debug.Log($"Door '{name}': assigned global QuestText automatically.");
        }
    }

    // Called by QuestManager when a quest is completed and unlocks doors that depend on quests
    public void OnQuestCompleted(QuestId quest)
    {
        Debug.Log($"Door '{name}' received quest completion event for quest: {quest}");
        
        if (unlockType != UnlockType.Quest) return;
        if (requiredQuest == QuestId.None) return;
        if (quest != requiredQuest) return;
        
        Unlock();
    }

    // Just unlocks the door and updates visuals
    public void Unlock()
    {
        if (state == DoorState.Locked)
        {
            state = DoorState.Open;
            UpdateVisuals();
            Debug.Log($"Door '{name}' unlocked.");
        }
    }

    // Tries to open the door (plays ANIMATION)
    public void TryOpenDoor()
    {
        if (state == DoorState.Locked)
        {
            Debug.Log($"Door '{name}' is locked. No animation.");
            return;
        }

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }
    }

    // Tries to close the door (plays ANIMATION)
    public void TryCloseDoor()
    {
        if (state == DoorState.Locked) return;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Closed");
        }
    }

    private void HandleQuestAndKeyDoor()
    {
        bool questCompleted = false;
        bool hasKey = false;

        // Check the quest
        if (requiredQuest != QuestId.None && QuestManager.Instance != null)
        {
            questCompleted = QuestManager.Instance.IsCompleted(requiredQuest);
        }

        // Check for the required key
        hasKey = HasRequiredKey();

        // Are both conditions met?
        if (questCompleted && hasKey)
        {
            UnlockWithKey();
        }
        else if (!questCompleted)
        {
            // Quest not completed
            if (questText != null)
            {
                questText.Show($"The door is closed. Quest required: {requiredQuest}");
            }
        }
        else if (!hasKey)
        {
            // Quest completed but no key available
            ShowNoKeyMessage();
        }
    }
}