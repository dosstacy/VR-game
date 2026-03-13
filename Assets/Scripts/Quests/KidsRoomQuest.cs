using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[Serializable]
public class PotSlot
{
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket;
    public PotColor expectedColor;
}

public class KidsRoomQuest : MonoBehaviour
{
    [SerializeField] private PotSlot[] slots;

    private bool completed;

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
            Debug.LogError("Slots array is empty!", this);

        foreach (var s in slots)
        {
            if (s.socket == null) Debug.LogError("One of slots has no socket assigned!", this);
            if (s.expectedColor == PotColor.None) Debug.LogError("One of slots has expectedColor=None!", this);
        }
    }

    private void OnEnable() //subscribe to socket events
    {
        if (slots == null) return;

        foreach (var s in slots)
        {
            if (s.socket == null) continue;
            s.socket.selectEntered.AddListener(OnAnySlotChanged);
            s.socket.selectExited.AddListener(OnAnySlotChanged);
        }
    }

    private void OnDisable() //unsubscribe from socket events
    {
        if (slots == null) return;

        foreach (var s in slots)
        {
            if (s.socket == null) continue;
            s.socket.selectEntered.RemoveListener(OnAnySlotChanged);
            s.socket.selectExited.RemoveListener(OnAnySlotChanged);
        }
    }

    private void OnAnySlotChanged(BaseInteractionEventArgs _)
    {
        if (completed) return;

        if (AllSlotsCorrect())
        {
            Debug.Log("All correct pots placed!");
            if (QuestManager.Instance != null)
            {
                completed = true;
                QuestManager.Instance.CompleteQuest(Door.QuestId.ChildRoom);
            }
        }
    }

    private bool AllSlotsCorrect()
    {
        return slots.All(IsSlotCorrect);
    }

    private bool IsSlotCorrect(PotSlot slot)
    {
        var pot = GetPotFromSocket(slot.socket);
        return pot != null && pot.color == slot.expectedColor;
    }

    private Pot GetPotFromSocket(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socket)
    {
        if (socket == null) return null;
        if (!socket.hasSelection) return null;

        var interactable = socket.interactablesSelected.FirstOrDefault();
        if (interactable == null) return null;

        return interactable.transform.GetComponent<Pot>();
    }
}
