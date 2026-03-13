using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Call `OnBackToMenuPressed()` from a UI Button. The script executes
/// the `onAutoSave` event (you can connect an existing SaveManager in the inspector),
/// waits for a short pause, and loads the menu scene.
/// </summary>
public class BackToMenu : MonoBehaviour
{
    [Tooltip("Name of the main menu scene")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Method to call for auto-saving")]
    public UnityEvent onAutoSave;

    [Tooltip("Time to wait after calling auto-save (seconds)")]
    public float waitAfterSaveSeconds = 0.5f;

    public void OnBackToMenuPressed()
    {
        StartCoroutine(DoSaveAndLoad());
    }

    private IEnumerator DoSaveAndLoad()
    {
        if (AutoSaveManager.Instance != null)
        {
            AutoSaveManager.Instance.SaveGame();
        }
        else
        {
            onAutoSave?.Invoke();
        }

        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SaveProgressToDisk();
        }

        float waited = 0f;
        var path = System.IO.Path.Combine(Application.persistentDataPath, "save.json");
        while (waited < Mathf.Max(0.1f, waitAfterSaveSeconds))
        {
            if (System.IO.File.Exists(path)) break;
            waited += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
