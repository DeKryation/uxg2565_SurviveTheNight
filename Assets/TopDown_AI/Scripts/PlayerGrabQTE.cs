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

    private Rigidbody playerRb;
    private RigidbodyConstraints originalConstraints;
    private NPC_Enemy capturingEnemy;

    private bool qteActive = false;
    private float needlePosition = 0f;
    private float zoneCenter = 0.65f;
    public bool IsActive => qteActive;

    private int successes = 0;
    private int misses = 0;

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

        // Automatically miss when the needle passes the zone.
        float zoneEnd = zoneCenter + successZoneWidth * 0.5f;

        if (needlePosition > zoneEnd)
            RegisterMiss();
    }

    public bool BeginQTE(NPC_Enemy enemy)
    {
        if (qteActive || enemy == null || qtePanel == null ||
            trackRect == null || needleRect == null ||
            successZoneRect == null)
            return false;

        qteActive = true;
        capturingEnemy = enemy;

        successes = 0;
        misses = 0;

        // Stop the player's movement and attack controls.
        if (playerBehavior != null)
            playerBehavior.enabled = false;

        SoundManager.StopFootsteps();

        if (playerRb != null)
        {
            originalConstraints = playerRb.constraints;
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.constraints = RigidbodyConstraints.FreezeAll;
        }

        enemy.BeginCapture();

        qtePanel.SetActive(true);

        if (instructionText != null)
            instructionText.text = "PRESS SPACE IN THE GREEN ZONE";

        StartNextCheck();

        return true;
    }

    void StartNextCheck()
    {
        needlePosition = 0f;

        // Randomise the target position each check.
        zoneCenter = Random.Range(0.35f, 0.8f);

        UpdateZoneVisual();
        UpdateNeedleVisual();
        UpdateProgressText();
    }

    void CheckTiming()
    {
        float halfWidth = successZoneWidth * 0.5f;

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
                "SUCCESS: " + successes + "/" + successesNeeded +
                "   MISSES: " + misses + "/" + maxMisses;
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
        if (trackRect == null || successZoneRect == null)
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
            // Restore player controls.
            if (playerRb != null)
            {
                playerRb.constraints = originalConstraints;
                playerRb.linearVelocity = Vector3.zero;
            }

            if (playerBehavior != null)
                playerBehavior.enabled = true;

            // Apply damage only AFTER the QTE is complete.
            // Perfect escape = no damage.
            // Escape with mistakes = 1 damage.
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }

            TensionDirector.ResetTension();

            // Zombie retreats after releasing the player.
            if (capturingEnemy != null)
                capturingEnemy.EndCapture(transform);
        }
        else
        {
            // All QTE attempts failed: immediate game over.
            if (playerHealth != null)
                playerHealth.ForceDeath();
            else if (playerBehavior != null)
                playerBehavior.DiePlayer();
        }

        capturingEnemy = null;
    }
}
