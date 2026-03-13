using System;
using UnityEngine;

/// <summary>
/// play when player enters the trigger.
/// Automatically hides after use or when quest is completed.
/// </summary>
public class AudioTriggerManager : MonoBehaviour
{
    [Tooltip("Audio clip to play")]
    public AudioClip audioClip;

    [Tooltip("Audio priority")]
    public NarrativeAudioManager.AudioPriority priority = NarrativeAudioManager.AudioPriority.Normal;

    [Header("Trigger Behavior")]
    [Tooltip("Hide trigger after use (permanently for this save)")]
    public bool hideAfterUse = true;

    [Tooltip("Unique ID of the trigger (generated automatically)")]
    [SerializeField] private string triggerId;

    [Header("Quest Binding")]
    [Tooltip("Automatically hide when this quest is completed (even if the player hasn't entered)")]
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

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(triggerId))
        {
            triggerId = Guid.NewGuid().ToString("N"); //line without "-"
        }
    }

    private void Start()
    {
        // if the trigger has already been used - hide it
        if (hideAfterUse && TriggerManager.Instance != null)
        {
            if (TriggerManager.Instance.IsTriggerDisabled(triggerId))
            {
                Debug.Log($"AudioTriggerManager '{name}': Already used, hiding.");
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
            Debug.Log($"AudioTriggerManager '{name}': Quest {relatedQuest} completed, auto-hiding trigger.");
            
            // hide and save
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
        if (audioClip == null) return;

        // if the quest is already completed - hide instead of playing
        if (hideWhenQuestCompleted && relatedQuest != Door.QuestId.None)
        {
            if (QuestManager.Instance != null && QuestManager.Instance.IsCompleted(relatedQuest))
            {
                Debug.Log($"AudioTriggerManager '{name}': Quest already completed, hiding.");
                
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

        // play audio
        if (NarrativeAudioManager.Instance != null)
        {
            NarrativeAudioManager.Instance.Play(audioClip, priority);
        }
        else
        {
            Debug.LogWarning($"AudioTriggerManager '{name}': NarrativeAudioManager not found!");
        }

        // hide trigger after use
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