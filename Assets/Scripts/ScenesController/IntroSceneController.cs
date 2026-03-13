using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controller for the intro scene (Scene 2) - plays audio and automatically transitions to the game.
/// </summary>
public class IntroSceneController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Audio")]
    [SerializeField] private AudioClip introAudioClip;

    [Header("Timing")]
    [Tooltip("How many seconds to wait after audio finishes before transitioning")]
    [SerializeField] private float delayAfterAudio = 5f;
    [SerializeField] private SceneController _sceneController; 
    private AudioSource audioSource;

    private void Start()
    {
        StartCoroutine(IntroSequence());
    }

    private IEnumerator IntroSequence()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.clip = introAudioClip;
        
            audioSource.Play();
            Debug.Log("Intro audio started.");

            // Wait for the audio to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            Debug.Log("Intro audio finished.");
        }

        // Delay after audio
        yield return new WaitForSeconds(delayAfterAudio);

        Debug.Log("Transitioning to game scene...");
        _sceneController.LoadScene(gameSceneName);
    }
}