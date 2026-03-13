using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OutroSceneController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioClip outroAudio;

    [Header("Timing")]
    [Tooltip("How many seconds to wait after outro audio finishes before returning to main menu")]
    [SerializeField] private float delayAfterAudio = 3f;
    [SerializeField] private SceneController _sceneController; 
    private AudioSource audioSource;

    private void Start()
    {
        StartCoroutine(OutroSequence());
    }

    private void Awake ()
    {
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 
        audioSource.volume = 1f;
    }

    private IEnumerator OutroSequence()
    {
        if (audioSource != null)
        {
            audioSource.clip = outroAudio;
            audioSource.Play();
            Debug.Log("Outro audio started.");

            // Wait for the audio to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            Debug.Log("Outro audio finished.");
        }

        // Delay after audio
        yield return new WaitForSeconds(delayAfterAudio);

        // Transition to main menu
        Debug.Log("Returning to main menu...");
        _sceneController.LoadScene(mainMenuSceneName);
    }
}