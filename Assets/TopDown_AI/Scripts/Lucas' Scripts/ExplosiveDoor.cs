using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ExplosiveDoor : MonoBehaviour
{
    [Header("Requirement")]
    public int explosivesRequired = 5;

    [Header("Door Behaviour")]
    public bool destroyDoorWhenOpened = true;
    public GameObject doorObjectToHide;
    public bool loadNextSceneWhenOpened = false;
    public string nextSceneName;

    [Header("Optional UI Message")]
    public TMP_Text messageText;
    public float messageDuration = 2f;

    private float messageHideTime = 0f;
    private bool opened = false;

    void Start()
    {
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (messageText != null && messageText.gameObject.activeSelf && Time.time >= messageHideTime)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (opened) return;

        if (!other.CompareTag("Player"))
            return;

        ExplosiveInventory inventory = other.GetComponent<ExplosiveInventory>();

        if (inventory == null)
        {
            ShowMessage("Missing ExplosiveInventory on Player");
            return;
        }

        if (inventory.explosivesCollected >= explosivesRequired)
        {
            OpenDoor();
        }
        else
        {
            ShowMessage(inventory.explosivesCollected + " / " + explosivesRequired + " Explosives Required");
        }
    }

    void OpenDoor()
    {
        opened = true;

        if (loadNextSceneWhenOpened)
        {
            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("ExplosiveDoor: Next Scene Name is empty.");
            }

            return;
        }

        if (doorObjectToHide != null)
        {
            doorObjectToHide.SetActive(false);
        }
        else if (destroyDoorWhenOpened)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void ShowMessage(string message)
    {
        if (messageText == null)
            return;

        messageText.text = message;
        messageText.gameObject.SetActive(true);
        messageHideTime = Time.time + messageDuration;
    }
}
