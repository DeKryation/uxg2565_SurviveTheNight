using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Trigger_LoadScene : MonoBehaviour
{
    public string sceneName;

    [Header("Transition Sound")]
    public bool playTransitionSound = true;
    public float delayBeforeLoad = 0.5f;

    private bool isLoading = false;

    void OnTriggerEnter(Collider user)
    {
        if (isLoading) return;

        if (user.CompareTag("Player"))
        {
            StartCoroutine(LoadSceneWithSound());
        }
    }

    IEnumerator LoadSceneWithSound()
    {
        isLoading = true;

        if (playTransitionSound)
        {
            SoundManager.PlaySceneTransition();
        }

        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(sceneName);
    }
}
