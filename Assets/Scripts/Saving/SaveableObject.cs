using System;
using UnityEngine;

/// <summary>
/// save game object position and rotation by unique ID
/// </summary>
public class SaveableObject : MonoBehaviour
{
    [Header("Object ID")]
    [Tooltip("Unique ID for this object (generated automatically)")]
    [SerializeField] private string objectId;

    [Header("What to Save")]
    [SerializeField] private bool savePosition = true;
    [SerializeField] private bool saveRotation = true;

    // #if UNITY_EDITOR
    // private void OnValidate()
    // {
    //     // Automatically generate ID
    //     if (string.IsNullOrWhiteSpace(objectId))
    //     {
    //         objectId = $"{gameObject.name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    //     }
    // }

    // [ContextMenu("Generate New Object ID")]
    // private void GenerateNewObjectId()
    // {
    //     objectId = $"{gameObject.name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    //     UnityEditor.EditorUtility.SetDirty(this);
    // }
    // #endif

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(objectId))
        {
            objectId = $"{gameObject.name}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }

    private void Start()
    {
        if (ObjectPositionManager.Instance != null)
        {
            ObjectPositionManager.Instance.RegisterObject(this);
        }
        LoadPosition();
    }

    private void OnDestroy()
    {
        // Unregister object on destroy
        if (ObjectPositionManager.Instance != null)
        {
            ObjectPositionManager.Instance.UnregisterObject(this);
        }
    }

    /// Save the current position of the object
    public ObjectTransformData GetTransformData()
    {
        return new ObjectTransformData
        {
            objectId = objectId,
            position = savePosition ? transform.position : Vector3.zero,
            rotation = saveRotation ? transform.rotation : Quaternion.identity,
            savePosition = savePosition,
            saveRotation = saveRotation
        };
    }

    /// Load the saved position
    public void LoadPosition()
    {
        if (ObjectPositionManager.Instance == null) return;

        var data = ObjectPositionManager.Instance.GetObjectData(objectId);
        if (data == null) return;

        // Restore the position
        if (data.savePosition)
        {
            transform.position = data.position;
        }

        if (data.saveRotation)
        {
            transform.rotation = data.rotation;
        }

        Debug.Log($"SaveableObject '{name}': Position loaded.");
    }

    public string ObjectId => objectId;
}

/// Data about the object's transform for saving
[Serializable]
public class ObjectTransformData
{
    public string objectId;
    public Vector3 position;
    public Quaternion rotation;
    public bool savePosition;
    public bool saveRotation;

    public float rotX => rotation.x;
    public float rotY => rotation.y;
    public float rotZ => rotation.z;
    public float rotW => rotation.w;
}