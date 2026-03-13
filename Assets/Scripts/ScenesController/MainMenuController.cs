using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene names")]
    [SerializeField] private string introSceneName = "IntroScene"; 
    [SerializeField] private string gameSceneName = "Game";

    [Header("UI")]
    [SerializeField] private GameObject aboutPanel;
    [SerializeField] private GameObject mainButtonsPanel;   
    [SerializeField] private Button continueButton;    
    [SerializeField] private SceneController _sceneController; 

    private void Start()
    {
        if (aboutPanel != null)
            aboutPanel.SetActive(false);

        if (continueButton != null)
            continueButton.interactable = SaveSystem.HasSave();
    }

    public void StartGame()
    {
        SaveSystem.DeleteSave();
        _sceneController.LoadScene(introSceneName);
        //SceneManager.LoadScene(introSceneName);
    }

    public void ContinueGame()
    {
        if (!SaveSystem.HasSave())
        {
            Debug.Log("No save found, Continue blocked.");
            return;
        }
        _sceneController.LoadScene(gameSceneName);
        //SceneManager.LoadScene(gameSceneName);
    }

    public void About()
    {
        if (aboutPanel == null) return;

        bool show = !aboutPanel.activeSelf;
        aboutPanel.SetActive(show);

        if (mainButtonsPanel != null)
            mainButtonsPanel.SetActive(!show);
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}