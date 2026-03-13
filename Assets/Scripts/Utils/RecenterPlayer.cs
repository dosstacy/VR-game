using UnityEngine;
using Unity.XR.CoreUtils;

public class RecenterPlayer : MonoBehaviour
{
    [SerializeField] private float targetYaw = 0f;

    void Start()
    {
        StartCoroutine(RecenterAfterDelay());
    }

    private System.Collections.IEnumerator RecenterAfterDelay()
    {
        // wait until XR tracking is initialized
        yield return new WaitForSeconds(0.5f);
        
        XROrigin xrOrigin = GetComponent<XROrigin>();
        Transform camera = xrOrigin.Camera.transform;
        
        Debug.Log("Camera yaw after delay: " + camera.eulerAngles.y);
        
        float currentYaw = camera.eulerAngles.y;
        transform.RotateAround(camera.position, Vector3.up, targetYaw - currentYaw);
    }
}