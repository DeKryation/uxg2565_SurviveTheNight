using UnityEngine;
using TMPro;

public class PlayerGrabQTE : MonoBehaviour
{
    [Header("QTE UI")]
    public GameObject qtePanel;
    public RectTransform trackRect;
    public RectTransform needleRect;
    public RectTransform successZoneRect;
    public TMP_Text progressText;
    public TMP_Text instructionText;

    [Header("QTE Settings")]
    public int successesNeeded = 3;
    public int maxMisses = 3;
    public float needleSpeed = 0.7f;

    [Range(0.05f, 0.4f)]
    public float successZoneWidth = 0.15f;

    [Header("References")]
    public PlayerBehavior playerBehavior;
    public PlayerHealth playerHealth;

    [Header("Progressive Jumpscares")]
    public JumpscareManager jumpscareManager;

    private Rigidbody playerRb;
    private RigidbodyConstraints originalConstraints;
    private NPC_Enemy capturingEnemy;

    private bool qteActive = false;
    private bool waitingForDeath = false;

    private float needlePosition = 0f;
    private float zoneCenter = 0.65f;

    private int successes = 0;
    private int misses = 0;

    // Used by doors to prevent interaction during QTE.
    public bool IsActive => qteActive || waitingForDeath;

    void Awake()
    {
        if (playerBehavior == null)
            playerBehavior = GetComponent<PlayerBehavior>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        playerRb = GetComponent<Rigidbody>();

        if (playerRb != null)
            originalConstraints = playerRb.constraints;

        if (qtePanel != null)
            qtePanel.SetActive(false);
    }

    void Update()
    {
        if (!qteActive || Time.timeScale <= 0f)
            return;

        needlePosition += needleSpeed * Time.deltaTime;
        needlePosition = Mathf.Clamp01(needlePosition);

        UpdateNeedleVisual();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckTiming();
            return;
        }

        // Automatically miss when the needle
        // moves past the green zone.
        float zoneEnd =
            zoneCenter + successZoneWidth * 0.5f;

        if (needlePosition > zoneEnd)
        {
            RegisterMiss();
        }
    }

    public bool BeginQTE(NPC_Enemy enemy)
    {
        if (qteActive ||
            waitingForDeath ||
            enemy == null ||
            qtePanel == null ||
            trackRect == null ||
            needleRect == null ||
            successZoneRect == null)
        {
            return false;
        }

        qteActive = true;
        capturingEnemy = enemy;

        successes = 0;
        misses = 0;

        // Disable player movement and attacks.
        if (playerBehavior != null)
            playerBehavior.enabled = false;

        SoundManager.StopFootsteps();

        // Freeze the player.
        if (playerRb != null)
        {
            originalConstraints = playerRb.constraints;

            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;

            playerRb.constraints =
                RigidbodyConstraints.FreezeAll;
        }

        // Zombie holds the player.
        enemy.BeginCapture();

        // Show QTE immediately.
        qtePanel.SetActive(true);

        if (instructionText != null)
        {
            instructionText.text =
                "PRESS SPACE IN THE GREEN ZONE";
        }

        // Play progressive jumpscare on every catch.
        // QTE continues normally underneath.
        if (jumpscareManager != null)
        {
            jumpscareManager.PlayCurrentJumpscare(
                null,
                true
            );
        }

        StartNextCheck();

        return true;
    }

    void StartNextCheck()
    {
        needlePosition = 0f;

        // Random target position.
        zoneCenter = Random.Range(0.35f, 0.8f);

        UpdateZoneVisual();
        UpdateNeedleVisual();
        UpdateProgressText();
    }

    void CheckTiming()
    {
        float halfWidth =
            successZoneWidth * 0.5f;

        bool successful =
            needlePosition >= zoneCenter - halfWidth &&
            needlePosition <= zoneCenter + halfWidth;

        if (successful)
        {
            successes++;

            if (successes >= successesNeeded)
            {
                FinishQTE(true);
                return;
            }

            StartNextCheck();
        }
        else
        {
            RegisterMiss();
        }
    }

    void RegisterMiss()
    {
        misses++;

        if (misses >= maxMisses)
        {
            FinishQTE(false);
            return;
        }

        StartNextCheck();
    }

    void UpdateProgressText()
    {
        if (progressText != null)
        {
            progressText.text =
                "SUCCESS: " +
                successes + "/" + successesNeeded +
                "   MISSES: " +
                misses + "/" + maxMisses;
        }
    }

    void UpdateNeedleVisual()
    {
        if (trackRect == null || needleRect == null)
            return;

        float width = trackRect.rect.width;

        needleRect.anchoredPosition = new Vector2(
            (needlePosition - 0.5f) * width,
            needleRect.anchoredPosition.y
        );
    }

    void UpdateZoneVisual()
    {
        if (trackRect == null ||
            successZoneRect == null)
            return;

        float width = trackRect.rect.width;

        successZoneRect.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Horizontal,
            width * successZoneWidth
        );

        successZoneRect.anchoredPosition = new Vector2(
            (zoneCenter - 0.5f) * width,
            successZoneRect.anchoredPosition.y
        );
    }

    void FinishQTE(bool escaped)
    {
        qteActive = false;

        if (qtePanel != null)
            qtePanel.SetActive(false);

        if (escaped)
        {
            // Restore Rigidbody constraints.
            if (playerRb != null)
            {
                playerRb.constraints =
                    originalConstraints;

                playerRb.linearVelocity =
                    Vector3.zero;
            }

            // Restore player controls.
            if (playerBehavior != null)
                playerBehavior.enabled = true;

            // Always lose 1 HP after escaping.
            // Even if every check was successful.
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }

            TensionDirector.ResetTension();

            // Zombie releases the player and flees.
            if (capturingEnemy != null)
            {
                capturingEnemy.EndCapture(transform);
            }

            capturingEnemy = null;
        }
        else
        {
            // Player failed the entire QTE.
            // Keep controls locked during death jumpscare.
            waitingForDeath = true;

            if (jumpscareManager != null)
            {
                bool started =
                    jumpscareManager.PlayDeathJumpscare(
                        TriggerQTEDeath
                    );

                if (!started)
                    TriggerQTEDeath();
            }
            else
            {
                TriggerQTEDeath();
            }
        }
    }

    void TriggerQTEDeath()
    {
        if (!waitingForDeath)
            return;

        waitingForDeath = false;

        // Prevent duplicate death events.
        if (playerHealth != null)
        {
            playerHealth.ForceDeath();
        }
        else if (playerBehavior != null)
        {
            playerBehavior.DiePlayer();
        }

        capturingEnemy = null;
    }
}
