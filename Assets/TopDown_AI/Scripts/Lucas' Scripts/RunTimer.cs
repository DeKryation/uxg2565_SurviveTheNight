using UnityEngine;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance;

    public static float timeSurvived = 0f;
    public static bool timerRunning = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (timerRunning)
        {
            timeSurvived += Time.deltaTime;
        }
    }

    public static void StartTimer()
    {
        timerRunning = true;
    }

    public static void StopTimer()
    {
        timerRunning = false;
    }

    public static void ResetTimer()
    {
        timeSurvived = 0f;
        timerRunning = false;
    }

    public static string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(timeSurvived / 60f);
        int seconds = Mathf.FloorToInt(timeSurvived % 60f);

        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}
