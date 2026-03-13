using UnityEngine;
using Unity.XR.CoreUtils;

public class ThrowZoneDoorTrigger : MonoBehaviour
{
    public VRThrowing playerThrowing;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<XROrigin>() == null) return;

        Debug.Log("Player entered throw zone, enabling throwing");
        playerThrowing.SetThrowEnabled(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<XROrigin>() == null) return;

        Debug.Log("Player exited throw zone, disabling throwing");
        playerThrowing.SetThrowEnabled(false);
    }
}
