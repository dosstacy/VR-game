using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for ending the game - plays outro audio and returns to the menu.
/// </summary>
public class GameEndController : MonoBehaviour
{
    public static GameEndController Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string outroSceneName = "OutroScene";

    [Header("Audio")]
    [SerializeField] private AudioClip outroAudioClip;
    [SerializeField] private float outroVolume = 1f;

    [Header("Timing")]
    [Tooltip("Delay before starting outro audio")]
    [SerializeField] private float delayBeforeAudio = 2f;
    
    [Tooltip("Delay before fade-out after outro audio finishes")]
    [SerializeField] private float delayAfterAudio = 3f;
    [SerializeField] private SceneController _sceneController;

    private bool gameEnded = false;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // create AudioSource
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = outroVolume;
    }

    /// Trigger the game end sequence
    public void TriggerGameEnd()
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("Game ended! Starting end sequence...");
        
        StartCoroutine(EndGameSequence());
    }

    private IEnumerator EndGameSequence()
    {
        // 1. Delay before audio
        Debug.Log($"Waiting {delayBeforeAudio}s before outro audio...");
        yield return new WaitForSeconds(delayBeforeAudio);

        // 2. Play outro audio
        if (outroAudioClip != null && audioSource != null)
        {
            audioSource.clip = outroAudioClip;
            audioSource.Play();
            Debug.Log("Outro audio started.");

            // Wait for the audio to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            Debug.Log("Outro audio finished.");
        }
        else
        {
            Debug.LogWarning("No outro audio clip assigned or AudioSource missing!");
        }

        yield return new WaitForSeconds(delayAfterAudio);

        Debug.Log("Returning to main menu...");
        _sceneController.LoadScene(outroSceneName);
    }
}