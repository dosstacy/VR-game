using UnityEngine;

public class TargetHit : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer targetRenderer;

    [Header("Materials")]
    public Material hitMaterial;

    private bool isHit = false;

    private void Reset()
    {
        targetRenderer = GetComponentInChildren<Renderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isHit) return;

        if (collision.gameObject.GetComponent<Projectile>() == null) return;

        isHit = true;

        if (targetRenderer != null && hitMaterial != null)
            targetRenderer.material = hitMaterial;

        TargetsManager.Instance?.RegisterHit();
    }
}
