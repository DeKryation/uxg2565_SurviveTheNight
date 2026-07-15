using UnityEngine;
using TMPro;

public class VictoryScreenTimer : MonoBehaviour
{
    public TMP_Text timeSurvivedText;

    void Start()
    {
        if (timeSurvivedText != null)
        {
            timeSurvivedText.text = RunTimer.GetFormattedTime();
        }
    }
}
