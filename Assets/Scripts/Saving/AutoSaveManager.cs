using UnityEngine;

/// <summary>
/// Autosave manager.
/// Automatically saves the game after completing quests.
/// </summary>
public class AutoSaveManager : MonoBehaviour
{
    public static AutoSaveManager Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Show save notification")]
    [SerializeField] private bool showSaveNotification = true;

    [Tooltip("UI for displaying the notification")]
    [SerializeField] private QuestText saveNotificationUI;

    [SerializeField] private string saveMessage = "Game saved";

    [Header("References")]
    [SerializeField] private PlayerPositionManager playerPositionManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (playerPositionManager == null)
        {
            playerPositionManager = FindObjectOfType<PlayerPositionManager>();
        }
    }

    public void SaveGame()
    {
        Debug.Log("AutoSaveManager: Saving game...");

        // 1. Save player position
        if (playerPositionManager != null)
        {
            playerPositionManager.SavePosition();
        }
        else
        {
            Debug.LogWarning("AutoSaveManager: PlayerPositionManager not found!");
        }

        // 2. Save object positions
        if (ObjectPositionManager.Instance != null)
        {
            ObjectPositionManager.Instance.SaveAllObjectPositions();
        }

        // 3. Save progress through QuestManager (quests, doors, keys, triggers)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.SaveProgressToDisk();
        }
        else
        {
            Debug.LogWarning("AutoSaveManager: QuestManager not found!");
        }

        // 4. Show save notification
        if (showSaveNotification && saveNotificationUI != null)
        {
            saveNotificationUI.Show(saveMessage);
        }

        Debug.Log("AutoSaveManager: Game saved successfully.");
    }

    // Save the game after completing a quest (called from QuestManager)
    public void OnQuestCompleted()
    {
        SaveGame();
    }
}