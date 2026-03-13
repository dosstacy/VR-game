using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    //path: c/Users/username/AppData/LocalLow/CompanyName/GameName/save.json
    private const string FileName = "save.json";

    private static string PathToSave =>
        System.IO.Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave() => File.Exists(PathToSave);

    public static void Save(SaveData data)
    {
        try
        {
            data.lastSaveUtc = DateTime.UtcNow.ToString("o");
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(PathToSave, json);
            Debug.Log($"Saved: {PathToSave}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e}");
        }
    }

    public static SaveData Load()
    {
        try
        {
            if (!HasSave()) return null;
            var json = File.ReadAllText(PathToSave);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e}");
            return null;
        }
    }

    public static void DeleteSave()
    {
        try
        {
            if (HasSave()) File.Delete(PathToSave);
        }
        catch (Exception e)
        {
            Debug.LogError($"DeleteSave failed: {e}");
        }
    }
}

[Serializable]
public class PlayerPosition
{
    public float x;
    public float y;
    public float z;
    public float rotationY;

    public PlayerPosition() { }

    public PlayerPosition(Vector3 position, float rotation)
    {
        x = position.x;
        y = position.y;
        z = position.z;
        rotationY = rotation;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[Serializable]
public class SaveData
{
    public string lastSaveUtc;
    public int[] completedQuestIds;
    public string[] visitedDoorIds;
    public int[] collectedKeys;
    public string[] collectedKeyIds;
    public string[] disabledTriggerIds;
    public PlayerPosition playerPosition;
    public SerializedObjectTransform[] objectTransforms; 
}