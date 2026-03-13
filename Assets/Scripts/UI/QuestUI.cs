using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private TMP_Text questText;

    public void SetQuest(string text)
    {
        if (questText) questText.text = text;
    }
}