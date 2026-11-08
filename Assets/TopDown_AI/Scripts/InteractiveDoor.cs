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

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;

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

        PlayerGrabQTE qte =
            player.GetComponent<PlayerGrabQTE>();

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
