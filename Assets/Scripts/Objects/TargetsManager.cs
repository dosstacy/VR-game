using UnityEngine;

public class TargetsManager : MonoBehaviour
{
    public static TargetsManager Instance { get; private set; }

    [Header("Targets")]
    public int totalTargets = 6;

    // [Header("Black screen")]
    // public GameObject blackScreen; 

    private int hitCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterHit()
    {
        hitCount++;

        if (hitCount >= totalTargets)
        {
            GameEndController.Instance.TriggerGameEnd();
            Debug.Log("All targets hit! You win! Quest complete.");
        }
    }
}
