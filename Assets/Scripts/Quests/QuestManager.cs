using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// QuestManager is responsible for tracking the player's progress in completing quests, 
/// as well as saving and loading this progress.
/// </summary>
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private QuestUI questUI;

    [Header("Doors")]
    [Tooltip("If empty, QuestManager will auto-find all Door components in the active scene.")]
    public List<Door> doors = new();

    private readonly HashSet<Door.QuestId> completed = new();

    private readonly HashSet<string> visitedDoorIds = new();
    private readonly HashSet<string> allDoorIdsInScene = new();

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

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded; //add as listener for scene load events
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded; //remove listener when disabled/destroyed

    private void Start()
    {
        LoadProgressFromDisk();
        RebuildSceneCaches();
        ApplyProgressToDoors();
        Debug.Log("ApplyProgressToDoors in start");
        Debug.Log(Application.persistentDataPath);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebuildSceneCaches();
        ApplyProgressToDoors();
        Debug.Log("ApplyProgressToDoors on scene loaded");
    }

    // -------- UI --------
    public void SetQuestText(string text)
    {
        if (questUI == null)
        {
            Debug.LogWarning("QuestUI is not assigned on QuestManager.");
            return;
        }
        questUI.SetQuest(text);
    }

    // -------- Quests --------
    public bool IsCompleted(Door.QuestId questId) => completed.Contains(questId);

    public void CompleteQuest(Door.QuestId questId)
    {
        if (questId == Door.QuestId.None) return;
        if (!completed.Add(questId)) return;

        // apply progress to doors
        ApplyProgressToDoors();
        
        // autosave
        if (AutoSaveManager.Instance != null)
        {
            AutoSaveManager.Instance.OnQuestCompleted();
        }
        else
        {
            // Fallback
            SaveProgressToDisk();
        }
        
        Debug.Log($"Quest completed: {questId}");
    }

    // -------- Door visits --------
    public void MarkDoorVisited(string doorId)
    {
        if (string.IsNullOrWhiteSpace(doorId)) return;

        if (visitedDoorIds.Add(doorId))
        {
            SaveProgressToDisk();
            Debug.Log($"Door visited: {doorId}. Unvisited in this scene: {GetUnvisitedDoorCount()}");
        }
    }

    public int GetUnvisitedDoorCount()
    {
        int count = 0;
        foreach (var id in allDoorIdsInScene)
            if (!visitedDoorIds.Contains(id))
                count++;
        return count;
    }

    public List<string> GetUnvisitedDoorIds()
    {
        var list = new List<string>();
        foreach (var id in allDoorIdsInScene)
            if (!visitedDoorIds.Contains(id))
                list.Add(id);
        return list;
    }

    // -------- Scene caches --------
    private void RebuildSceneCaches()
    {
        // doors
        doors.Clear();
        doors.AddRange(FindObjectsOfType<Door>(true));

        // all door trigger ids in this scene
        allDoorIdsInScene.Clear();
        var triggers = FindObjectsOfType<TriggerDoor>(true);
        foreach (var t in triggers)
        {
            if (t == null) continue;
            if (!string.IsNullOrWhiteSpace(t.DoorId))
                allDoorIdsInScene.Add(t.DoorId);
        }
    }

    private void ApplyProgressToDoors()
    {
        foreach (var door in doors)
        {
            if (door == null) continue;
            foreach (var quest in completed)
                door.OnQuestCompleted(quest);
        }
    }

    // ---------------- SAVE / LOAD ----------------
    private void LoadProgressFromDisk()
    {
        var data = SaveSystem.Load();
        if (data == null) return;

        // quests
        completed.Clear();
        if (data.completedQuestIds != null)
        {
            foreach (var id in data.completedQuestIds)
            {
                var quest = (Door.QuestId)id;
                if (quest != Door.QuestId.None)
                    completed.Add(quest);
            }
        }

        // visited doors
        visitedDoorIds.Clear();
        if (data.visitedDoorIds != null)
        {
            foreach (var id in data.visitedDoorIds)
                if (!string.IsNullOrWhiteSpace(id))
                    visitedDoorIds.Add(id);
        }
    }

    public void SaveProgressToDisk()
    {
        SaveSystem.Save(ExportSaveData());
    }

    public SaveData ExportSaveData()
    {
        //quest
        var questArr = new int[completed.Count];
        int i = 0;
        foreach (var q in completed) questArr[i++] = (int)q;

        // doors
        var doorArr = new string[visitedDoorIds.Count];
        i = 0;
        foreach (var id in visitedDoorIds) doorArr[i++] = id;

        // keys
        int[] keysArr = null;
        string[] keyIdsArr = null;
        if (KeyManager.Instance != null)
        {
            keysArr = KeyManager.Instance.ExportKeyTypes();
            keyIdsArr = KeyManager.Instance.ExportKeyIds();
        }

        // disabled triggers
        string[] disabledTriggersArr = null;
        if (TriggerManager.Instance != null)
        {
            disabledTriggersArr = TriggerManager.Instance.ExportDisabledTriggers();
        }

        // object positions
        SerializedObjectTransform[] objectTransformsArr = null;
        if (ObjectPositionManager.Instance != null)
        {
            objectTransformsArr = ObjectPositionManager.Instance.ExportObjectTransforms();
        }

        // player position
        PlayerPosition playerPos = null;
        var existingSave = SaveSystem.Load();
        if (existingSave != null)
        {
            playerPos = existingSave.playerPosition;
        }

        return new SaveData
        {
            completedQuestIds = questArr,
            visitedDoorIds = doorArr,
            collectedKeys = keysArr,
            collectedKeyIds = keyIdsArr,
            disabledTriggerIds = disabledTriggersArr,
            playerPosition = playerPos,
            objectTransforms = objectTransformsArr
        };
    }
}