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
    [Tooltip("Optional: the solid collider on the door mesh. Distance is measured to its closest point, not the hinge.")]
    public Collider doorCollider;
    public bool debugInteraction = false;

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
    private PlayerBehavior currentPlayerBehavior;
    private KeyInventory keyInventory;
    private AudioSource audioSource;

    [Header("Special Door Unlock SFX")]
    public AudioClip insufficientExplosivesSFX;

    [Range(0f, 1f)]
    public float insufficientExplosivesVolume = 1f;

    [Header("Explosive Requirement")]
    public int explosivesRequired = 4;

    // Prevent repeated special SFX after the first unlock.
    private bool hasPlayedUnlockSFX = false;

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

        RefreshPlayerReference();

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

    // Resolve the actual active player controller, not merely any GameObject tagged Player.
    // This also ignores leftover disabled player controllers from the death sequence.
    void RefreshPlayerReference()
    {
        PlayerBehavior[] controllers = FindObjectsByType<PlayerBehavior>(
            FindObjectsSortMode.None
        );

        PlayerBehavior best = null;
        float bestDistance = float.PositiveInfinity;

        foreach (PlayerBehavior controller in controllers)
        {
            if (controller == null || !controller.isActiveAndEnabled)
                continue;

            // Only controllers in the same loaded scene as this door.
            if (controller.gameObject.scene != gameObject.scene)
                continue;

            float dist = Vector3.SqrMagnitude(controller.transform.position - transform.position);
            if (dist < bestDistance)
            {
                best = controller;
                bestDistance = dist;
            }
        }

        currentPlayerBehavior = best;
        player = best != null ? best.transform : null;
        keyInventory = best != null ? best.GetComponent<KeyInventory>() : null;
    }

    float GetInteractionDistance()
    {
        if (player == null) return float.PositiveInfinity;
        Vector3 point = doorCollider != null
            ? doorCollider.ClosestPoint(player.position)
            : transform.position;
        // Ground-plane range, for the game's top-down movement.
        Vector3 diff = player.position - point;
        diff.y = 0f;
        return diff.magnitude;
    }

    void HandlePlayerInteraction()
    {
        if (!Input.GetKeyDown(interactionKey))
            return;

        RefreshPlayerReference();

        float distance = GetInteractionDistance();

        if (debugInteraction)
        {
            Debug.Log("[DOOR DEBUG] PlayerController=" +
                (player != null ? player.name : "MISSING") +
                " | PlayerPos=" + (player != null ? player.position.ToString() : "N/A") +
                " | DoorPos=" + transform.position +
                " | Distance=" + distance.ToString("F2") +
                " | Range=" + interactionDistance, this);
        }

        if (player == null || distance > interactionDistance)
            return;

        PlayerGrabQTE qte = player.GetComponent<PlayerGrabQTE>();
        if (qte != null && qte.IsActive)
            return;

        if (keyInventory == null || keyInventory.keysCollected < keysRequired)
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

        // Play the normal door opening sound.
        PlayDoorSound(doorOpenSFX, openVolume);

        // Play the special SFX on the first successful unlock.
        if (!hasPlayedUnlockSFX)
        {
            CheckExplosivesAndPlaySFX();
            hasPlayedUnlockSFX = true;
        }
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
    void CheckExplosivesAndPlaySFX()
    {
        GameObject currentPlayer =
            GameObject.FindGameObjectWithTag("Player");

        if (currentPlayer == null)
            return;

        ExplosiveInventory inventory =
            currentPlayer.GetComponent<ExplosiveInventory>();

        if (inventory == null)
            return;

        // Only play when fewer than 4 explosives are collected.
        if (inventory.explosivesCollected < explosivesRequired)
        {
            PlayDoorSound(
                insufficientExplosivesSFX,
                insufficientExplosivesVolume
            );
        }
    }


}