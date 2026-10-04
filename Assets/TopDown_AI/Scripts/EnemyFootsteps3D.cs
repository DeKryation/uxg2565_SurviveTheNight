using UnityEngine;
using UnityEngine.AI;

public class EnemyFootsteps3D : MonoBehaviour
{
    [Header("Footstep Audio")]
    public AudioClip footstepLoopSFX;
    [Range(0f, 1f)] public float footstepVolume = 0.8f;

    [Header("Zombie Groans")]
    public AudioClip[] zombieGroanSFX;
    [Range(0f, 1f)] public float groanVolume = 0.9f;
    public float minGroanDelay = 4f;
    public float maxGroanDelay = 9f;

    [Header("3D Distance Settings")]
    public float minDistance = 2f;
    public float maxDistance = 18f;

    [Header("Movement Detection")]
    public float movementThreshold = 0.1f;

    [Header("Optional Pitch")]
    public bool adjustPitchBySpeed = true;
    public float normalSpeed = 7f;
    public float minPitch = 0.85f;
    public float maxPitch = 1.25f;

    private AudioSource footstepSource;
    private AudioSource groanSource;
    private NavMeshAgent navMeshAgent;

    private float nextGroanTime = 0f;

    void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        footstepSource = gameObject.AddComponent<AudioSource>();
        Setup3DAudioSource(footstepSource);
        footstepSource.clip = footstepLoopSFX;
        footstepSource.loop = true;
        footstepSource.playOnAwake = false;
        footstepSource.volume = footstepVolume;

        groanSource = gameObject.AddComponent<AudioSource>();
        Setup3DAudioSource(groanSource);
        groanSource.loop = false;
        groanSource.playOnAwake = false;
        groanSource.volume = groanVolume;
    }

    void Start()
    {
        SetNextGroanTime();
    }

    void Update()
    {
        HandleFootsteps();
        HandleRandomGroans();
    }

    void HandleFootsteps()
    {
        if (footstepLoopSFX == null || navMeshAgent == null)
            return;

        bool isMoving = navMeshAgent.velocity.magnitude > movementThreshold && Time.timeScale > 0f;

        if (isMoving)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.clip = footstepLoopSFX;
                footstepSource.Play();
            }

            if (adjustPitchBySpeed)
            {
                float speedPercent = navMeshAgent.velocity.magnitude / normalSpeed;
                footstepSource.pitch = Mathf.Clamp(speedPercent, minPitch, maxPitch);
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }

    void HandleRandomGroans()
    {
        if (zombieGroanSFX == null || zombieGroanSFX.Length == 0)
            return;

        if (Time.timeScale <= 0f)
            return;

        if (Time.time >= nextGroanTime)
        {
            PlayRandomGroan();
            SetNextGroanTime();
        }
    }

    void PlayRandomGroan()
    {
        if (groanSource.isPlaying)
            return;

        AudioClip chosenGroan = zombieGroanSFX[
            Random.Range(0, zombieGroanSFX.Length)
        ];

        if (chosenGroan == null)
            return;

        groanSource.pitch = Random.Range(0.9f, 1.1f);
        groanSource.PlayOneShot(chosenGroan, groanVolume);
    }

    void SetNextGroanTime()
    {
        nextGroanTime = Time.time + Random.Range(minGroanDelay, maxGroanDelay);
    }

    void Setup3DAudioSource(AudioSource source)
    {
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
    }

    public void StopFootsteps()
    {
        if (footstepSource != null && footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    public void StopAllEnemyAudio()
    {
        if (footstepSource != null)
            footstepSource.Stop();

        if (groanSource != null)
            groanSource.Stop();
    }
}
