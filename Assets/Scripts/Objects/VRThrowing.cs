using UnityEngine;
using UnityEngine.InputSystem;

public class VRThrowing : MonoBehaviour
{
    public Transform cam;
    public Transform attackPoint;
    public GameObject objectToThrow;

    public int totalThrows = 1000;
    public float throwCooldown = 0.1f;

    public float throwForce = 15f;
    public float throwUpwardForce = 0f;

    public InputActionProperty throwAction;

    private bool readyToThrow = true;
    private bool canThrow = false;

    private void OnEnable() => throwAction.action.Enable();
    private void OnDisable() => throwAction.action.Disable();

    private void Update()
    {
        if (!canThrow || !readyToThrow || totalThrows <= 0) return;

        if (throwAction.action.WasPressedThisFrame())
            Throw();
    }

    public void SetThrowEnabled(bool enabled) => canThrow = enabled;

    private void Throw()
    {
        Debug.Log("Throwing object");
        readyToThrow = false;

        var projectile = Instantiate(objectToThrow, attackPoint.position, cam.rotation);

        var rb = projectile.GetComponent<Rigidbody>();
        if (!rb)
        {
            Debug.LogError("ObjectToThrow has no Rigidbody");
            Destroy(projectile);
            readyToThrow = true;
            return;
        }

        Vector3 forceDirection = attackPoint.forward;

        if (Physics.Raycast(attackPoint.position, attackPoint.forward, out RaycastHit hit, 500f))
            forceDirection = (hit.point - attackPoint.position).normalized;

        Vector3 forceToAdd =
            forceDirection * throwForce +
            attackPoint.up * throwUpwardForce;

        rb.AddForce(forceToAdd, ForceMode.Impulse);

        totalThrows--;
        Invoke(nameof(ResetThrow), throwCooldown);
    }

    private void ResetThrow() => readyToThrow = true;
}
