using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance { get; private set; }

    private readonly HashSet<KeyType> collectedKeyTypes = new();

    private readonly HashSet<string> collectedKeyIds = new();

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
        LoadKeysFromSave();
    }

    public void CollectKey(KeyType keyType, string keyId)
    {
        bool typeAdded = collectedKeyTypes.Add(keyType);
        bool idAdded = collectedKeyIds.Add(keyId);

        if (typeAdded || idAdded)
        {
            Debug.Log($"Key collected: {keyType} (ID: {keyId})");
            SaveKeys();
        }
    }

    public bool HasKey(KeyType keyType)
    {
        return collectedKeyTypes.Contains(keyType);
    }


    public bool IsKeyCollected(string keyId)
    {
        return collectedKeyIds.Contains(keyId);
    }

    public bool HasAnyKey()
    {
        return collectedKeyTypes.Count > 0;
    }

    public int GetKeyCount()
    {
        return collectedKeyTypes.Count;
    }

    private void LoadKeysFromSave()
    {
        var saveData = SaveSystem.Load();
        if (saveData == null) return;

        collectedKeyTypes.Clear();
        if (saveData.collectedKeys != null)
        {
            foreach (var key in saveData.collectedKeys)
            {
                collectedKeyTypes.Add((KeyType)key);
            }
        }

        collectedKeyIds.Clear();
        if (saveData.collectedKeyIds != null)
        {
            foreach (var id in saveData.collectedKeyIds)
            {
                collectedKeyIds.Add(id);
            }
        }

        Debug.Log($"Loaded {collectedKeyTypes.Count} key types and {collectedKeyIds.Count} specific keys from save.");
    }

    private void SaveKeys()
    {
        if (QuestManager.Instance == null) return;
        QuestManager.Instance.SaveProgressToDisk();
    }

    // for export to SaveData
    public int[] ExportKeyTypes()
    {
        var keys = new int[collectedKeyTypes.Count];
        int i = 0;
        foreach (var key in collectedKeyTypes)
        {
            keys[i++] = (int)key;
        }
        return keys;
    }

    public string[] ExportKeyIds()
    {
        var ids = new string[collectedKeyIds.Count];
        collectedKeyIds.CopyTo(ids);
        return ids;
    }
}