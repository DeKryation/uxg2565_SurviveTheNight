using UnityEngine;

public class LockedDoor : MonoBehaviour
{
    [Header("Door References")]
    public Transform doorVisual;

    [Header("Door Settings")]
    public float openAngle = 90f;
    public float rotationSpeed = 150f;
    public bool startOpen = false;

    [Header("Player Interaction")]
    public float interactionDistance = 2.5f;
    public KeyCode interactionKey = KeyCode.E;

    [Header("Key Requirement")]
    public int keysRequired = 1;

    [Header("Door Audio")]
    public AudioClip doorOpenSFX;
    public AudioClip doorCloseSFX;
    public AudioClip doorFailedSFX;

    [Range(0f, 1f)] public float openVolume = 0.8f;
    [Range(0f, 1f)] public float closeVolume = 0.8f;
    [Range(0f, 1f)] public float failedVolume = 0.8f;

    public float minDistance = 3f;
    public float maxDistance = 25f;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    private bool isOpen;
    private Transform player;
    private KeyInventory keyInventory;
    private AudioSource audioSource;

    void Start()
    {
        if (doorVisual == null)
            doorVisual = transform;

        closedRotation = doorVisual.localRotation;

        openRotation = closedRotation *
            Quaternion.Euler(0f, openAngle, 0f);

        isOpen = startOpen;

        doorVisual.localRotation =
            isOpen ? openRotation : closedRotation;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            keyInventory =
                playerObject.GetComponent<KeyInventory>();
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        HandlePlayerInteraction();
        RotateDoor();
    }

    void HandlePlayerInteraction()
    {
        if (player == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        if (distance > interactionDistance)
            return;

        if (!Input.GetKeyDown(interactionKey))
            return;

        // Prevent opening doors during QTE.
        PlayerGrabQTE qte =
            player.GetComponent<PlayerGrabQTE>();

        if (qte != null && qte.IsActive)
            return;

        // Door is locked without the required key.
        if (keyInventory == null ||
            keyInventory.keysCollected < keysRequired)
        {
            PlayDoorSound(doorFailedSFX, failedVolume);

            Debug.Log("Door is locked. A key is required!");
            return;
        }

        ToggleDoor();
    }

    void RotateDoor()
    {
        Quaternion targetRotation =
            isOpen ? openRotation : closedRotation;

        doorVisual.localRotation = Quaternion.RotateTowards(
            doorVisual.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    public void OpenDoor()
    {
        if (isOpen)
            return;

        isOpen = true;

        PlayDoorSound(doorOpenSFX, openVolume);
    }

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;

        PlayDoorSound(doorCloseSFX, closeVolume);
    }

    void PlayDoorSound(AudioClip clip, float clipVolume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, clipVolume);
        }
    }
}