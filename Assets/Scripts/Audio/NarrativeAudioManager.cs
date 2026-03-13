using System.Collections;
using UnityEngine;

/// <summary>
/// global manager for narrative audio clips with priorities.
/// </summary>
public class NarrativeAudioManager : MonoBehaviour
{
    public static NarrativeAudioManager Instance { get; private set; }

    public enum AudioPriority
    {
        Normal = 0,      //  Atmosphere - skipped if something is playing
        Important = 1,   // Hints - interrupts Normal, blocks Normal
        Critical = 2     // Narrative dialogues - interrupts everything except Critical, blocks everything
    }

    [Header("Settings")]
    [SerializeField] private float delayBetweenClips = 0.2f;

    private AudioSource audioSource;
    private AudioPriority currentPriority = AudioPriority.Normal;
    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void Play(AudioClip clip, AudioPriority priority)
    {
        if (clip == null) return;

        // If something is playing with a higher or equal priority - ignore the new one
        if (isPlaying && currentPriority >= priority)
        {
            Debug.Log($"NarrativeAudioManager: Skipped '{clip.name}' " +
                      $"(priority {priority}, current: {currentPriority})");
            return;
        }

        // Stop current audio if the new one has a higher priority
        if (isPlaying && priority > currentPriority)
        {
            StopAllCoroutines();
            audioSource.Stop();
            Debug.Log($"NarrativeAudioManager: Interrupted by higher priority " +
                      $"(was {currentPriority}, now {priority})");
        }

        StartCoroutine(PlayAudio(clip, priority));
    }

    private IEnumerator PlayAudio(AudioClip clip, AudioPriority priority)
    {
        isPlaying = true;
        currentPriority = priority;
        
        Debug.Log($"NarrativeAudioManager: Playing '{clip.name}' (priority: {priority})");

        yield return new WaitForSeconds(delayBetweenClips);

        audioSource.clip = clip;
        audioSource.Play();

        // wait until the end
        yield return new WaitWhile(() => audioSource.isPlaying);

        isPlaying = false;
        currentPriority = AudioPriority.Normal;
        Debug.Log($"NarrativeAudioManager: Finished '{clip.name}'");
    }

    //"getters" for other scripts to check status
    public bool IsPlaying => isPlaying;
    public AudioPriority CurrentPriority => currentPriority;
}