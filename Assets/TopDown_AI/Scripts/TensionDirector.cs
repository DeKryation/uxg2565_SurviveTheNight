using UnityEngine;
using UnityEngine.UI;


public enum TensionLevel
{
    LOW,
    MEDIUM,
    HIGH
}

public class TensionDirector : MonoBehaviour
{
    public static TensionDirector Instance;

    [Header("Tension Value")]
    public float currentTension = 0f;
    public float maxTension = 100f;

    [Header("Tension Build Up")]
    public float tensionIncreasePerSecond = 3f;

    [Header("Thresholds")]
    public float mediumThreshold = 40f;
    public float highThreshold = 75f;

    [Header("UI")]
    public Slider tensionSlider;
    public Image tensionFillImage;
    public Color lowColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color highColor = Color.red;

    public TensionLevel CurrentLevel { get; private set; }

    private TensionLevel previousLevel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateTensionUI();
        previousLevel = CurrentLevel;
        UpdateTensionUI();
    }

    void Update()
    {
        currentTension += tensionIncreasePerSecond * Time.deltaTime;
        currentTension = Mathf.Clamp(currentTension, 0f, maxTension);

        UpdateTensionLevel();
        UpdateTensionUI();
    }

    void UpdateTensionLevel()
    {
        TensionLevel newLevel;

        if (currentTension >= highThreshold)
        {
            newLevel = TensionLevel.HIGH;
        }
        else if (currentTension >= mediumThreshold)
        {
            newLevel = TensionLevel.MEDIUM;
        }
        else
        {
            newLevel = TensionLevel.LOW;
        }

        if (newLevel != CurrentLevel)
        {
            CurrentLevel = newLevel;
            PlayTensionLevelSFX(CurrentLevel);
        }
        else
        {
            CurrentLevel = newLevel;
        }
    }

    void PlayTensionLevelSFX(TensionLevel level)
    {
        if (level == TensionLevel.LOW)
        {
            SoundManager.PlayTensionLow();
        }
        else if (level == TensionLevel.MEDIUM)
        {
            SoundManager.PlayTensionMedium();
        }
        else if (level == TensionLevel.HIGH)
        {
            SoundManager.PlayTensionHigh();
        }
    }

    void UpdateTensionUI()
    {
        if (tensionSlider != null)
        {
            tensionSlider.maxValue = maxTension;
            tensionSlider.value = currentTension;
        }

        if (tensionFillImage != null)
        {
            if (CurrentLevel == TensionLevel.HIGH)
                tensionFillImage.color = highColor;
            else if (CurrentLevel == TensionLevel.MEDIUM)
                tensionFillImage.color = mediumColor;
            else
                tensionFillImage.color = lowColor;
        }
    }

    public static TensionLevel GetTensionLevel()
    {
        if (Instance == null)
            return TensionLevel.LOW;

        return Instance.CurrentLevel;
    }

    public static void ResetTension()
    {
        if (Instance == null)
            return;

        Instance.currentTension = 0f;

        TensionLevel oldLevel = Instance.CurrentLevel;

        Instance.UpdateTensionLevel();
        Instance.UpdateTensionUI();

        if (oldLevel != TensionLevel.LOW)
        {
            SoundManager.PlayTensionLow();
        }
    }
}
