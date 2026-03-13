using System;
using System.Collections;
using UnityEngine;

public class TriggerQuestText : MonoBehaviour
{
    [TextArea(2, 4)]
    public string message;

    [SerializeField] private QuestText toastUI;

    [Header("Trigger Behavior")]
    [Tooltip("Hide trigger after use (permanently for this save)")]
    public bool hideAfterUse = true;

    [Tooltip("Unique ID of the trigger (generated automatically)")]
    [SerializeField] private string triggerId;

    [Header("Quest Binding")]
    [Tooltip("Automatically hide when this quest is completed (even if the player hasn't visited)")]
    public bool hideWhenQuestCompleted = false;
    
    [Tooltip("Quest after which the trigger is hidden")]
    public Door.QuestId relatedQuest = Door.QuestId.None;

    private bool hasCheckedQuest = false;

    // #if UNITY_EDITOR
    // private void OnValidate()
    // {
    //     // Automatically generate ID
    //     if (string.IsNullOrWhiteSpace(triggerId))
    //     {
    //         triggerId = Guid.NewGuid().ToString("N");
    //     }
    // }

    // [ContextMenu("Generate New Trigger ID")]
    // private void GenerateNewTriggerId()
    // {
    //     triggerId = Guid.NewGuid().ToString("N");
    //     UnityEditor.EditorUtility.SetDirty(this);
    // }
    // #endif

    void Awake()
    {
        if (string.IsNullOrWhiteSpace(triggerId))
        {
            triggerId = Guid.NewGuid().ToString("N");
        }
    }

    private void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    private void Start()
    {
        // If the trigger has already been used - hide it
        if (hideAfterUse && TriggerManager.Instance != null)
        {
            if (TriggerManager.Instance.IsTriggerDisabled(triggerId))
            {
                Debug.Log($"TriggerQuestText '{name}': Already used, hiding.");
                gameObject.SetActive(false);
                return;
            }
        }

        // Check if the quest is already completed
        CheckQuestStatus();
    }

    private void Update()
    {
        // Continuously check the quest status (if enabled)
        if (hideWhenQuestCompleted && !hasCheckedQuest)
        {
            CheckQuestStatus();
        }
    }

    private void CheckQuestStatus()
    {
        if (!hideWhenQuestCompleted || relatedQuest == Door.QuestId.None) return;
        if (hasCheckedQuest) return;

        if (QuestManager.Instance != null && QuestManager.Instance.IsCompleted(relatedQuest))
        {
            hasCheckedQuest = true;
            Debug.Log($"TriggerQuestText '{name}': Quest {relatedQuest} completed, auto-hiding trigger.");
            
            // Hide and save
            if (TriggerManager.Instance != null)
            {
                TriggerManager.Instance.MarkTriggerUsedAndDisable(triggerId, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // If the quest is already completed - hide instead of showing text
        if (hideWhenQuestCompleted && relatedQuest != Door.QuestId.None)
        {
            if (QuestManager.Instance != null && QuestManager.Instance.IsCompleted(relatedQuest))
            {
                Debug.Log($"TriggerQuestText '{name}': Quest already completed, hiding.");
                
                if (TriggerManager.Instance != null)
                {
                    TriggerManager.Instance.MarkTriggerUsedAndDisable(triggerId, gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                return;
            }
        }

        // Show the text
        if (toastUI)
        {
            toastUI.Show(message);
        }
        else
        {
            Debug.LogWarning($"TriggerQuestText '{name}': toastUI is not assigned.");
        }

        // Hide the trigger after use
        if (hideAfterUse)
        {
            if (TriggerManager.Instance != null)
            {
                TriggerManager.Instance.MarkTriggerUsedAndDisable(triggerId, gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}