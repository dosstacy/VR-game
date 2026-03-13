using System.Collections;
using TMPro;
using UnityEngine;

public class QuestText : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform head;          // HMD camera
    [SerializeField] private TMP_Text textField;
    [SerializeField] private CanvasGroup canvasGroup; 
    [SerializeField] private GameObject root;         

    [Header("Placement")]
    [SerializeField] private float distance = 1f;   
    [SerializeField] private float height = -0.15f;  

    [Header("Timing")]
    [SerializeField] private float showSeconds = 10f;

    [Header("Fade")]
    [SerializeField] private float fadeIn = 0.15f;
    [SerializeField] private float fadeOut = 0.25f;

    private Coroutine routine;

    private void Awake()
    {
        HideInstant();
    }

    public void Show(string message)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        if (!head || !textField || !canvasGroup || !root)
        {
            Debug.LogWarning("QuestToastUI: missing references.");
            yield break;
        }

        // position + rotation in moment of showing
        PlaceInFrontOfHead();

        textField.text = message;
        root.SetActive(true);

        // fade in
        yield return Fade(0f, 1f, fadeIn);

        //display for a while
        float t = 0f;
        while (t < showSeconds)
        {
            t += Time.deltaTime;

            // moving with the head
            PlaceInFrontOfHead();

            yield return null;
        }

        // fade out
        yield return Fade(1f, 0f, fadeOut);

        root.SetActive(false);
        routine = null;
    }

    private void PlaceInFrontOfHead()
    {
        Vector3 pos = head.position + head.forward * distance + head.up * height;
        transform.position = pos;

        // return to head
        Vector3 toHead = head.position - transform.position;
        toHead.y = 0f; 
        if (toHead.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(-toHead.normalized, Vector3.up);
    }

    private IEnumerator Fade(float from, float to, float dur)
    {
        dur = Mathf.Max(0.001f, dur);
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < dur)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / dur);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void HideInstant()
    {
        if (root) root.SetActive(false);
        if (canvasGroup) canvasGroup.alpha = 0f;
    }
}
