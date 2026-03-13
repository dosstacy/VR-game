using UnityEngine;

/// <summary>
/// Class for playing happy song 
/// </summary>
[RequireComponent(typeof(Collider))]
public class RoomMusicController : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip musicClip;

    [Header("Quest Activation")]
    [Tooltip("Music starts only after completing this quest")]
    [SerializeField] private bool requireQuestCompletion = false;
    [SerializeField] private Door.QuestId requiredQuest = Door.QuestId.None;

    [Header("Settings")]
    [Tooltip("Play from start when entering room or continue from pause")]
    [SerializeField] private bool restartOnEnter = false;

    [Tooltip("fade in/out")]
    [SerializeField] private bool useFade = true;

    [Tooltip("Time fade in/out")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Volume")]
    [SerializeField] private float targetVolume = 1f;

    private bool playerInRoom = false;
    private float currentVolume = 0f;
    private float fadeVelocity = 0f;
    private bool isActivated = false;
    private AudioSource musicSource;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    private void Awake()
    {
        if (requireQuestCompletion)
        {
            isActivated = false;
        }
        else
        {
            isActivated = true;
        }
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f; // 2D sound
            musicSource.clip = musicClip;
        }
    }

    private void Start()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            Debug.LogError("RoomMusicController: AudioSource not assigned!", this);
            return;
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0f;
        currentVolume = 0f;

        if (requireQuestCompletion)
        {
            isActivated = false;
            CheckQuestStatus();
        }
        else
        {
            isActivated = true;
        }
    }

    private void Update()
    {
        if (musicSource == null || !isActivated) return;

        if (useFade)
        {
            // Smooth volume change
            float targetVol = playerInRoom ? targetVolume : 0f;
            currentVolume = Mathf.SmoothDamp(currentVolume, targetVol, ref fadeVelocity, fadeDuration);
            musicSource.volume = currentVolume;

            // Stop music if volume is very low and player left
            if (!playerInRoom && currentVolume < 0.01f && musicSource.isPlaying)
            {
                musicSource.Pause();
            }
        }
    }

    private void CheckQuestStatus()
    {
        if (!requireQuestCompletion) return;

        if (QuestManager.Instance != null && QuestManager.Instance.IsCompleted(requiredQuest))
        {
            ActivateMusic();
        }
    }

    public void ActivateMusic()
    {
        if (isActivated) return;

        isActivated = true;
        Debug.Log($"RoomMusicController activated for quest: {requiredQuest}");

        bool playerIsPhysicallyHere = CheckIfPlayerIsInside();
        
        if (playerInRoom || playerIsPhysicallyHere)
        {
            playerInRoom = true; 
            StartMusic();
        }
    }

    //check if player is inside the collider (in case quest is completed while player is already in the room)
    private bool CheckIfPlayerIsInside()
    {
        var col = GetComponent<Collider>();
        if (col == null) return false;

        // Search for the player within the collider's bounds
        Collider[] hits = Physics.OverlapBox(
            col.bounds.center,
            col.bounds.extents,
            transform.rotation
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
                return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isActivated) 
        {
            Debug.Log("RoomMusicController: Not activated yet (quest not completed).");
            return;
        }
        if (musicSource == null) return;

        playerInRoom = true;
        StartMusic();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (musicSource == null) return;

        playerInRoom = false;

        if (!useFade)
        {
            musicSource.Pause();
        }

        Debug.Log("Player left room. Music: paused");
    }

    private void StartMusic()
    {
        if (restartOnEnter)
        {
            musicSource.Stop();
            musicSource.Play();
        }
        else
        {
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }

        if (!useFade)
        {
            musicSource.volume = targetVolume;
        }

        Debug.Log($"Player entered room. Music: {(restartOnEnter ? "restarted" : "resumed")}");
    }

    // public void PlayMusic()
    // {
    //     if (musicSource != null && !musicSource.isPlaying && isActivated)
    //     {
    //         musicSource.Play();
    //     }
    // }

    // public void StopMusic()
    // {
    //     if (musicSource != null)
    //     {
    //         musicSource.Stop();
    //     }
    // }

    // public void PauseMusic()
    // {
    //     if (musicSource != null)
    //     {
    //         musicSource.Pause();
    //     }
    // }

    public bool IsActivated => isActivated;
}