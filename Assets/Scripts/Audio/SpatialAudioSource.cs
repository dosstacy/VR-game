using UnityEngine;

/// <summary>
/// custom spatial audio source that adjusts volume based on player distance and settings.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SpatialAudioSource : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Distance Settings")]
    [Tooltip("Distance at which sound has maximum volume")]
    [SerializeField] private float minDistance = 1f;

    [Tooltip("Distance at which sound is completely silent")]
    [SerializeField] private float maxDistance = 20f;

    [Header("Volume Settings")]
    [Tooltip("Maximum volume (when player is close)")]
    [SerializeField] private float maxVolume = 1f;

    [Tooltip("Minimum volume (when player is far)")]
    [SerializeField] private float minVolume = 0f;

    [Header("Falloff")]
    [Tooltip("How quickly sound fades: Linear, Logarithmic, Custom")]
    [SerializeField] private AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    private AudioSource audioSource;

    private void Start()
    {
        Debug.Log("SpatialAudioSource started on " + gameObject.name);
        audioSource = GetComponent<AudioSource>();

        // configure AudioSource for 3D sound
        audioSource.spatialBlend = 1f; // 1 = fully 3D
        audioSource.rolloffMode = rolloffMode;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        // Find the player if not specified
        if (player == null)
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                player = mainCamera.transform;
            }
            else
            {
                Debug.LogWarning("SpatialAudioSource: Player/Camera not found!");
            }
        }
    }

    private void Update()
    {
        if (player == null || audioSource == null) return;

        //Debug.Log($"Updating SpatialAudioSource '{gameObject.name}': Player distance = {Vector3.Distance(transform.position, player.position):F2}");
        // Calculate distance
        float distance = Vector3.Distance(transform.position, player.position);

        // Calculate volume based on distance
        float volumeFactor = CalculateVolumeFactor(distance);
        audioSource.volume = Mathf.Lerp(minVolume, maxVolume, volumeFactor);
    }

    private float CalculateVolumeFactor(float distance)
    {
        if (distance <= minDistance)
            return 1f;

        if (distance >= maxDistance)
            return 0f;

        // Normalize distance between min and max
        float normalizedDistance = (distance - minDistance) / (maxDistance - minDistance);

        // Depending on rolloff mode
        switch (rolloffMode)
        {
            case AudioRolloffMode.Linear:
                return 1f - normalizedDistance;

            case AudioRolloffMode.Logarithmic:
                return 1f - Mathf.Log10(1f + normalizedDistance * 9f);

            default:
                return 1f - normalizedDistance;
        }
    }
}