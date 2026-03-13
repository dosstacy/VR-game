using System.Collections.Generic;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    public static TriggerManager Instance { get; private set; }

    private readonly HashSet<string> disabledTriggerIds = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDisabledTriggers();
    }

    public void MarkTriggerUsedAndDisable(string triggerId, GameObject triggerObject)
    {
        if (string.IsNullOrWhiteSpace(triggerId)) return;

        if (disabledTriggerIds.Add(triggerId))
        {
            SaveDisabledTriggers();
            Debug.Log($"TriggerManager: Trigger '{triggerId}' marked as used.");
        }

        if (triggerObject != null)
            triggerObject.SetActive(false);
    }

    // public static void MarkTriggerUsedAndDisable(string triggerId, GameObject triggerObject)
    // {
    //     if (Instance != null)
    //         Instance.MarkTriggerUsedAndDisable(triggerId, triggerObject);
    //     else if (triggerObject != null)
    //         triggerObject.SetActive(false);
    // }

    public bool IsTriggerDisabled(string triggerId)
    {
        return disabledTriggerIds.Contains(triggerId);
    }

    public void ResetAllTriggers()
    {
        disabledTriggerIds.Clear();
        SaveDisabledTriggers();
        Debug.Log("TriggerManager: All triggers reset.");
    }

    private void LoadDisabledTriggers()
    {
        var saveData = SaveSystem.Load();
        if (saveData == null || saveData.disabledTriggerIds == null) return;

        disabledTriggerIds.Clear();
        foreach (var id in saveData.disabledTriggerIds)
        {
            if (!string.IsNullOrWhiteSpace(id))
                disabledTriggerIds.Add(id);
        }

        Debug.Log($"TriggerManager: Loaded {disabledTriggerIds.Count} disabled triggers.");
    }

    private void SaveDisabledTriggers()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.SaveProgressToDisk();
    }

    public string[] ExportDisabledTriggers()
    {
        var arr = new string[disabledTriggerIds.Count];
        disabledTriggerIds.CopyTo(arr);
        return arr;
    }
}