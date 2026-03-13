using System;
using UnityEngine;

public class TriggerDoor : MonoBehaviour
{
    [Header("Door visit tracking")]
    [SerializeField] private string doorId;

    private Door _door;
    private bool _visitedThisSession;

    public string DoorId => doorId;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        if (c) c.isTrigger = true;
    }

    // #if UNITY_EDITOR
    //     private void OnValidate()
    //     {
    //         if (string.IsNullOrWhiteSpace(doorId))
    //             doorId = Guid.NewGuid().ToString("N");
    //     }

    //     [ContextMenu("Generate New Door Id")]
    //     private void GenerateNewDoorId()
    //     {
    //         doorId = Guid.NewGuid().ToString("N");
    //     }
    // #endif

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(doorId))
            doorId = Guid.NewGuid().ToString("N");
    }

    void Start()
    {
        _door = GetComponentInParent<Door>();

        if (_door == null)
            Debug.LogError("Door script not found in parent!", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _door.TryOpenDoor();

        if (!_visitedThisSession)
        {
            _visitedThisSession = true;
            if (QuestManager.Instance != null)
                QuestManager.Instance.MarkDoorVisited(doorId);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _door.TryCloseDoor();
    }
}
