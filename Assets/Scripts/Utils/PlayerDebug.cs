using UnityEngine;

public class VRPositionDebugger : MonoBehaviour
{
    //private CharacterController cc;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Vector3 lastPos;
    
    void Start()
    {
        //cc = GetComponent<CharacterController>();

        lastPosition = transform.position;
        lastRotation = transform.rotation;
        lastPos = transform.position;

        // Find all objects near X=32
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        
        Debug.Log("=== OBJECTS NEAR X=32 ===");
        foreach (GameObject obj in allObjects)
        {
            if (Mathf.Abs(obj.transform.position.x - 32f) < 5f) // within 5 meters of X=32
            {
                Debug.Log($"Object: {obj.name} | Position: {obj.transform.position} | Tag: {obj.tag}");
            }
        }
    }
    
    void Update()
    {
        Vector3 posBefore = transform.position;
        
        if (transform.position != lastPos)
        {
            float dist = Vector3.Distance(transform.position, lastPos);
            if (dist > 1f)
            {
                Debug.LogError($"Transform.position changed externally!");
                
                Component[] components = GetComponents<Component>();
                Debug.LogError("=== ALL COMPONENTS ON XR ORIGIN ===");
                foreach (Component comp in components)
                {
                    Debug.LogError($"- {comp.GetType().Name} (enabled: {(comp is Behaviour ? ((Behaviour)comp).enabled.ToString() : "N/A")})");
                }
            }
            lastPos = transform.position;
        }

        Vector3 currentPos = transform.position;
        Quaternion currentRot = transform.rotation;
        float distance = Vector3.Distance(currentPos, lastPosition);
        float angleDiff = Quaternion.Angle(lastRotation, currentRot);
        
        if (distance > 1f)
        {
            Debug.LogError($"=== TELEPORT DETECTED ===");
            Debug.LogError($"Position: {lastPosition} → {currentPos}");
            Debug.LogError($"Distance: {distance}m");
            Debug.LogError($"Delta X: {currentPos.x - lastPosition.x}");
            Debug.LogError($"Delta Y: {currentPos.y - lastPosition.y}");
            Debug.LogError($"Delta Z: {currentPos.z - lastPosition.z}");
            Debug.LogError($"Rotation changed: {angleDiff} degrees");
            //Debug.LogError($"IsGrounded: {cc.isGrounded}");
            Debug.LogError($"Frame: {Time.frameCount}");
            
            // Check all components that might be causing the movement
            var moveProvider = GetComponent<UnityEngine.XR.Interaction.Toolkit.ContinuousMoveProviderBase>();
            var snapTurn = GetComponent<UnityEngine.XR.Interaction.Toolkit.SnapTurnProviderBase>();
            //var bodyTransformer = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRBodyTransformer>();
            
            Debug.LogError($"Move Provider enabled: {(moveProvider != null ? moveProvider.enabled.ToString() : "null")}");
            Debug.LogError($"Snap Turn enabled: {(snapTurn != null ? snapTurn.enabled.ToString() : "null")}");
            //Debug.LogError($"Body Transformer enabled: {(bodyTransformer != null ? bodyTransformer.enabled.ToString() : "null")}");
        }
        
        lastPosition = currentPos;
        lastRotation = currentRot;
    }
    
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log($"[COLLISION] Object: {hit.gameObject.name} | Position: {hit.point} | Normal: {hit.normal}");
    }
}