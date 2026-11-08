using UnityEngine;
using TMPro;

public class KeyInventory : MonoBehaviour
{
    [Header("Key Collection")]
    public int keysCollected = 0;
    public int keysNeeded = 1;

    [Header("Key UI")]
    public TMP_Text keyText;

    private static int savedKeysCollected = 0;

    void Start()
    {
        keysCollected = savedKeysCollected;
        UpdateKeyUI();
    }

    public void AddKey(int amount = 1)
    {
        keysCollected += amount;
        keysCollected = Mathf.Clamp(keysCollected, 0, keysNeeded);

        savedKeysCollected = keysCollected;

        UpdateKeyUI();
    }

    public bool HasEnoughKeys()
    {
        return keysCollected >= keysNeeded;
    }

    public static void ResetSavedKeys()
    {
        savedKeysCollected = 0;
    }

    public static int GetSavedKeys()
    {
        return savedKeysCollected;
    }

    void UpdateKeyUI()
    {
        if (keyText != null)
        {
            keyText.text = keysCollected + " / " + keysNeeded + "\nCollected";
        }
    }
}
