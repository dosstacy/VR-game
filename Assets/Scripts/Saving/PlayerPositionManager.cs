using UnityEngine;

/// <summary>
/// Save and load the player's position.
/// </summary>
public class PlayerPositionManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("XR Origin or player object to move")]
    [SerializeField] private Transform playerTransform;

    private void Start()
    {
        if (playerTransform == null)
        {
            playerTransform = transform;
        }

        // Load position when the game starts
        LoadPosition();
    }

    /// save player position.
    public void SavePosition()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogWarning("PlayerPositionManager: QuestManager not found!");
            return;
        }

        Vector3 pos = playerTransform.position;
        float rot = playerTransform.eulerAngles.y;

        PlayerPosition playerPos = new PlayerPosition(pos, rot);

        // Save through QuestManager
        var saveData = QuestManager.Instance.ExportSaveData();
        saveData.playerPosition = playerPos;
        SaveSystem.Save(saveData);

        Debug.Log($"Player position saved: {pos}, rotation: {rot}");
    }

    /// Load the saved player position.
    public void LoadPosition()
    {
        var saveData = SaveSystem.Load();
        var cc = playerTransform.GetComponent<CharacterController>();

        if (saveData == null || saveData.playerPosition == null)
        {
            cc.enabled = false;
            playerTransform.position = new Vector3(82.749f, 5.042f, -87.46f);
            playerTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
            cc.enabled = true;
            Debug.Log("No saved player position found.");
            return;
        }

        Vector3 pos = saveData.playerPosition.ToVector3();
        float rot = saveData.playerPosition.rotationY;

        Debug.Log($"Loaded player position: {pos}, rotation: {rot}");

        if (cc != null)
        {
            Debug.Log("Disabling CharacterController to set player position.");
            cc.enabled = false;
            playerTransform.position = pos;
            playerTransform.rotation = Quaternion.Euler(0f, rot, 0f);
            cc.enabled = true;
        }
        else
        {
            playerTransform.position = pos;
            playerTransform.rotation = Quaternion.Euler(0f, rot, 0f);
        }

        Debug.Log($"Player position loaded: {pos}, rotation: {rot}");
    }
}