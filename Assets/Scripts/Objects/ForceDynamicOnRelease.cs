using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class ForceDynamicOnRelease : MonoBehaviour
{
    Rigidbody rb;
    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    }

    void OnEnable()  => grab.selectExited.AddListener(OnReleased);
    void OnDisable() => grab.selectExited.RemoveListener(OnReleased);

    void OnReleased(SelectExitEventArgs _)
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;       
        rb.angularVelocity = Vector3.zero;
    }
}
