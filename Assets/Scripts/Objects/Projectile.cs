using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Sticky settings")]
    [Tooltip("Minimum impact speed to stick (0 to stick always)")]
    public float minImpactSpeed = 0.0f;
    public bool ignoreTriggers = true;

    [Header("Safety")]
    [Tooltip("Ignore collisions for the first N seconds after spawning (so it doesn't stick to your hand/spawn point).")]
    public float armTime = 0.05f;
    public float lifetime = 10f;

    private Rigidbody rb;
    private bool stuck = false;
    private float spawnTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        spawnTime = Time.time;
    }

    private void Start() => Destroy(gameObject, lifetime);

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck) return;

        // collision protection immediately after Instantiate (hand/spawn point)
        if (Time.time - spawnTime < armTime) return;

        var otherCol = collision.collider;
        if (ignoreTriggers && otherCol.isTrigger) return;

        if (collision.relativeVelocity.magnitude < minImpactSpeed) return;

        Stick(collision);
    }

    private void Stick(Collision collision)
    {
        stuck = true;

        // get first contact
        ContactPoint cp = collision.GetContact(0);

        // fix position and slightly push out to avoid getting stuck in the surface
        transform.position = cp.point + cp.normal * 0.01f;

        // rotate the projectile so it "looks" away from the surface
        transform.rotation = Quaternion.LookRotation(-cp.normal);

        // stop the physics and make kinematic so it doesn't fall or get pushed around
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // so it doesn't stick and generate new collisions
        var myCol = GetComponent<Collider>();
        myCol.enabled = false;

        // if the surface is moving — the projectile will move with it
        transform.SetParent(collision.transform, true);
    }
}
