using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SafeTeleportation : MonoBehaviour
{
    private CharacterController cc;
    private ContinuousMoveProviderBase moveProvider;
    private Vector3 lastValidGroundedPosition;
    private const float maxFallDistance = 2f;
    
    void Start()
    {
        cc = GetComponent<CharacterController>();
        moveProvider = GetComponent<ContinuousMoveProviderBase>();
        lastValidGroundedPosition = transform.position;
    }
    
    void LateUpdate()
    {
        // If we fell too far - reset position
        if (transform.position.y < lastValidGroundedPosition.y - maxFallDistance)
        {
            Debug.LogWarning("Fell through floor! Resetting position.");
            cc.enabled = false;
            transform.position = new Vector3(
                transform.position.x, 
                lastValidGroundedPosition.y, 
                transform.position.z
            );
            cc.enabled = true;
            }
        }
}
