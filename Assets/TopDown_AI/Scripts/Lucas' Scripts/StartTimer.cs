using UnityEngine;

public class StartRunTimer : MonoBehaviour
{
    public bool resetTimerOnStart = true;

    void Start()
    {
        if (resetTimerOnStart)
        {
            RunTimer.ResetTimer();
        }

        RunTimer.StartTimer();
    }
}
