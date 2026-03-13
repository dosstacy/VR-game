using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manager for saving object positions in the game.
/// Automatically saves all objects with the SaveableObject component.
/// </summary>
public class ObjectPositionManager : MonoBehaviour
{
    public static ObjectPositionManager Instance { get; private set; }

    private readonly List<SaveableObject> registeredObjects = new();
    private readonly Dictionary<string, ObjectTransformData> loadedData = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // load saved data as early as possible so that any SaveableObject
        // that runs in Start() can query it without racing.
        LoadAllObjectPositions();
    }

    private void Start()
    {
    }


    public void RegisterObject(SaveableObject obj)
    {
        if (!registeredObjects.Contains(obj))
        {
            registeredObjects.Add(obj);
            Debug.Log($"ObjectPositionManager: Registered '{obj.name}'");
        }
    }

    public void UnregisterObject(SaveableObject obj)
    {
        registeredObjects.Remove(obj);
    }

    public ObjectTransformData GetObjectData(string objectId)
    {
        if (loadedData.TryGetValue(objectId, out var data))
        {
            return data;
        }
        return null;
    }

    public void SaveAllObjectPositions()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("ObjectPositionManager: QuestManager not found!");
            return;
        }

        Debug.Log($"ObjectPositionManager: Saving {registeredObjects.Count} objects...");

        QuestManager.Instance.SaveProgressToDisk();
    }

    /// Load positions of all objects from the save file
    private void LoadAllObjectPositions()
    {
        var saveData = SaveSystem.Load();
        if (saveData == null || saveData.objectTransforms == null)
        {
            Debug.Log("ObjectPositionManager: No saved object positions found.");
            return;
        }

        loadedData.Clear();

        // Deserialize data
        foreach (var serialized in saveData.objectTransforms)
        {
            var data = DeserializeObjectData(serialized);
            if (data != null)
            {
                loadedData[data.objectId] = data;
            }
        }

        Debug.Log($"ObjectPositionManager: Loaded {loadedData.Count} object positions.");
    }

    /// Export data for SaveData (called from QuestManager)
    public SerializedObjectTransform[] ExportObjectTransforms()
    {
        var list = new List<SerializedObjectTransform>();

        foreach (var obj in registeredObjects)
        {
            if (obj == null) continue;

            var data = obj.GetTransformData();
            list.Add(SerializeObjectData(data));
        }

        return list.ToArray();
    }

    // Serialization for JSON 
    private SerializedObjectTransform SerializeObjectData(ObjectTransformData data)
    {
        return new SerializedObjectTransform
        {
            objectId = data.objectId,
            posX = data.position.x,
            posY = data.position.y,
            posZ = data.position.z,
            rotX = data.rotation.x,
            rotY = data.rotation.y,
            rotZ = data.rotation.z,
            rotW = data.rotation.w,
            savePosition = data.savePosition,
            saveRotation = data.saveRotation
        };
    }

    private ObjectTransformData DeserializeObjectData(SerializedObjectTransform serialized)
    {
        return new ObjectTransformData
        {
            objectId = serialized.objectId,
            position = new Vector3(serialized.posX, serialized.posY, serialized.posZ),
            rotation = new Quaternion(serialized.rotX, serialized.rotY, serialized.rotZ, serialized.rotW),
            savePosition = serialized.savePosition,
            saveRotation = serialized.saveRotation
        };
    }
}


[System.Serializable]
public class SerializedObjectTransform
{
    public string objectId;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public bool savePosition;
    public bool saveRotation;
}