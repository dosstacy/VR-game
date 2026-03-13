using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// For smooth grabbing and releasing of cable ends
/// Increases drag when grabbed and resets it after release to prevent flying away
/// </summary>
public class CableEndControl : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private RigidbodyConstraints originalConstraints;
    
    void Start()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        originalConstraints = rb.constraints;
        
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }
    
    void OnGrab(SelectEnterEventArgs args)
    {
        // Increase drag when grabbed
        rb.linearDamping = 10f;
        rb.angularDamping = 10f;
    }
    
    void OnRelease(SelectExitEventArgs args)
    {
        // Stop the velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Reset the drag after a second
        Invoke("ResetDrag", 1f);
    }
    
    void ResetDrag()
    {
        rb.linearDamping = 1f;
        rb.angularDamping = 1f;
    }
}