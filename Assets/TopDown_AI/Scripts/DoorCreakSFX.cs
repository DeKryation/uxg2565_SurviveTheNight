using UnityEngine;

public class DoorCreakSFX : MonoBehaviour
{
    [Header("Door Creak SFX")]
    public AudioClip doorCreakSFX;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Touch Settings")]
    public bool allowPlayer = true;
    public bool allowEnemy = true;
    public float replayCooldown = 1.5f;

    [Header("3D Audio")]
    public float minDistance = 3f;
    public float maxDistance = 25f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    private AudioSource audioSource;
    private float nextTimeCanPlay = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;
    }

    void OnTriggerEnter(Collider other)
    {
        TryPlayFromObject(other.gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        TryPlayFromObject(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryPlayFromObject(collision.gameObject);
    }

    void OnCollisionStay(Collision collision)
    {
        TryPlayFromObject(collision.gameObject);
    }

    void TryPlayFromObject(GameObject otherObject)
    {
        bool isPlayer = allowPlayer && otherObject.CompareTag("Player");
        bool isEnemy = allowEnemy && otherObject.CompareTag("Enemy");

        if (!isPlayer && !isEnemy)
            return;

        PlayCreak();
    }

    public void PlayCreak()
    {
        if (Time.time < nextTimeCanPlay)
            return;

        if (doorCreakSFX == null)
        {
            Debug.LogWarning("DoorCreakSFX: Missing door creak audio clip.");
            return;
        }

        audioSource.PlayOneShot(doorCreakSFX, volume);
        nextTimeCanPlay = Time.time + replayCooldown;

        if (showDebugLogs)
            Debug.Log("Door creak SFX played.");
    }
}
