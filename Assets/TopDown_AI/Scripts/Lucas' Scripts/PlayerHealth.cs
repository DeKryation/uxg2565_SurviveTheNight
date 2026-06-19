using UnityEngine;
using UnityEngine.UI;

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

    private float nextTimeCanTakeDamage = 0f;
    private bool isDead = false;
    private PlayerBehavior playerBehavior;

    void Awake()
    {
        playerBehavior = GetComponent<PlayerBehavior>();
        currentHealth = maxHealth;
    }

    void Start()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount = 1)
    {
        if (isDead) return;
        if (Time.time < nextTimeCanTakeDamage) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        SoundManager.PlayPlayerHit();

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
    }
}
