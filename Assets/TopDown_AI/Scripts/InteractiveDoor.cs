using UnityEngine;

public class InteractiveDoor : MonoBehaviour
{
    [Header("Door")]
    public Transform doorVisual;
    public float openAngle = 90f;
    public float rotationSpeed = 150f;
    public bool startOpen = false;

    [Header("Player Interaction")]
    public float interactionDistance = 2.5f;
    public KeyCode interactionKey = KeyCode.E;
    [Tooltip("Optional: the solid collider on the door mesh. Distance is measured to its closest point, not the hinge.")]
    public Collider doorCollider;
    public bool debugInteraction = false;

    [Header("Enemy Interaction")]
    public bool enemyCanOpen = true;
    public float enemyOpenDistance = 2f;

    [Header("Door Audio")]
    public AudioClip doorOpenSFX;
    public AudioClip doorCloseSFX;

    [Range(0f, 1f)] public float volume = 0.8f;
    public float minDistance = 3f;
    public float maxDistance = 25f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpen;

    private Transform player;
    private PlayerBehavior currentPlayerBehavior;
    private Transform enemy;
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

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;

        RefreshPlayerReference();

        GameObject enemyObject =
            GameObject.FindGameObjectWithTag("Enemy");

        if (enemyObject != null)
            enemy = enemyObject.transform;
    }

    void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        HandlePlayerInteraction();
        HandleEnemyInteraction();
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

        ToggleDoor();
    }

    void HandleEnemyInteraction()
    {
        if (!enemyCanOpen || enemy == null || isOpen)
            return;

        float distance = Vector3.Distance(
            enemy.position,
            transform.position
        );

        if (distance <= enemyOpenDistance)
            OpenDoor();
    }

    void RotateDoor()
    {
        Quaternion target =
            isOpen ? openRotation : closedRotation;

        doorVisual.localRotation = Quaternion.RotateTowards(
            doorVisual.localRotation,
            target,
            rotationSpeed * Time.deltaTime
        );
    }

    public void ToggleDoor()
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

        PlayDoorSound(doorOpenSFX);
    }

    public void CloseDoor()
    {
        if (!isOpen)
            return;

        isOpen = false;

        PlayDoorSound(doorCloseSFX);
    }

    void PlayDoorSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }
}
