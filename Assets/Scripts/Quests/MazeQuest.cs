using UnityEngine;

/// <summary>
/// When player enters the trigger in the maze, the doors open and the quest is completed.
/// </summary>
[RequireComponent(typeof(Collider))]
public class MazeQuest : MonoBehaviour
{
    [Tooltip("Door to open")] 
    public Door[] doorsToOpen;

    [Tooltip("If specified, this QuestId will be marked as completed")] 
    public Door.QuestId questToComplete = Door.QuestId.Maze;

    [Tooltip("Activated only once, then deactivated")]
    private bool triggered = false;

    private void Start()
    {
        var collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            collider.isTrigger = true;
            Debug.Log("DoorUnlocker: set collider to IsTrigger mode.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        UnlockAndComplete();
    }

    [ContextMenu("Unlock Doors and Complete Quest")]
    public void UnlockAndComplete()
    {
        if (doorsToOpen != null)
        {
            foreach (var door in doorsToOpen)
            {
                if (door == null) continue;
                door.Unlock();
                Debug.Log($"DoorUnlocker: unlocked '{door.name}'");
            }
        }

        if (QuestManager.Instance != null && questToComplete != Door.QuestId.None)
        {
            Debug.Log($"DoorUnlocker: completing quest {questToComplete}");
            QuestManager.Instance.CompleteQuest(questToComplete);
        }
        else if (questToComplete != Door.QuestId.None)
        {
            Debug.LogWarning("DoorUnlocker: cannot complete quest - QuestManager missing");
        }
    }
}
