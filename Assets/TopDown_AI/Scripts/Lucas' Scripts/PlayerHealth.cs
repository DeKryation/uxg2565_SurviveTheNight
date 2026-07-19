using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI")]
    public Slider healthSlider;
    public Image healthFillImage;

    [Header("Damage Protection")]
    public float invincibleTimeAfterHit = 0.35f;

    [Header("Low Health Screen Effect")]
    public GameObject lowHealthOverlay;
    public int lowHealthThreshold = 1;

    private float nextTimeCanTakeDamage = 0f;
    private bool isDead = false;
    private PlayerBehavior playerBehavior;

    [Header("Player Hit Flash")]
    public Renderer playerRenderer;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.2f;

    private Color originalColor;
    private Coroutine hitFlashRoutine;

    void Awake()
    {
        playerBehavior = GetComponent<PlayerBehavior>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        currentHealth = maxHealth;

        if (lowHealthOverlay != null)
        {
            lowHealthOverlay.SetActive(false);
        }

        if (playerRenderer == null)
        {
            playerRenderer = GetComponentInChildren<Renderer>();
        }

        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (isDead) return;
        if (Time.time < nextTimeCanTakeDamage) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SoundManager.PlayPlayerHit();
        FlashPlayerRed();

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            isDead = true;

            if (playerBehavior != null)
            {
                playerBehavior.DiePlayer();
            }
            else
            {
                gameObject.SetActive(false);
            }

            return;
        }

        nextTimeCanTakeDamage = Time.time + invincibleTimeAfterHit;
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    public void ResetHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        nextTimeCanTakeDamage = 0f;
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = maxHealth <= 0 ? 0 : (float)currentHealth / maxHealth;
        }

        if (lowHealthOverlay != null)
        {
            lowHealthOverlay.SetActive(currentHealth <= lowHealthThreshold);
        }
    }
    void FlashPlayerRed()
    {
        if (playerRenderer == null)
            return;

        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
        }

        hitFlashRoutine = StartCoroutine(HitFlashRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        playerRenderer.material.color = hitColor;

        yield return new WaitForSeconds(hitFlashDuration);

        playerRenderer.material.color = originalColor;
    }
}
