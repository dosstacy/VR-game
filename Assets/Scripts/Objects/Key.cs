using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public enum KeyType
{
    Key1,
    Key2
}

public class Key : MonoBehaviour
{
    [Header("Key Settings")]
    public KeyType keyType;

    [Header("Unique ID")]
    [Tooltip("Unique ID of this key in the scene (generated automatically)")]
    [SerializeField] private string keyId;

    [Header("UI")]
    [SerializeField] private QuestText questText;
    [SerializeField] private string collectMessage = "Key saved!";

    private XRGrabInteractable grabInteractable;
    private bool isCollected = false;

    // #if UNITY_EDITOR
    // private void OnValidate()
    // {
    //     // Automatically generate an ID if it is not specified
    //     if (string.IsNullOrWhiteSpace(keyId))
    //     {
    //         keyId = $"{gameObject.scene.name}_{keyType}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    //     }
    // }

    // [ContextMenu("Generate New Key ID")]
    // private void GenerateNewKeyId()
    // {
    //     keyId = $"{gameObject.scene.name}_{keyType}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    //     UnityEditor.EditorUtility.SetDirty(this);
    // }
    // #endif
    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            keyId = $"{gameObject.scene.name}_{keyType}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnKeyGrabbed);
        }

        // Check if key with this ID is already collected
        if (KeyManager.Instance != null && KeyManager.Instance.IsKeyCollected(keyId))
        {
            Debug.Log($"Key '{keyId}' already collected, hiding.");
            gameObject.SetActive(false);
        }
    }

    //grab and hide key
    private void OnKeyGrabbed(SelectEnterEventArgs args)
    {
        if (isCollected) return;

        isCollected = true;

        // Save the key with its unique ID
        if (KeyManager.Instance != null)
        {
            KeyManager.Instance.CollectKey(keyType, keyId);
        }

        // Show collection message that the key has been collected
        if (questText != null)
        {
            questText.Show(collectMessage);
        }

        // Hide the key after a short delay
        Invoke(nameof(HideKey), 0.5f);
    }

    private void HideKey()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnKeyGrabbed);
        }
    }

    public string GetKeyId() => keyId;
}