using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameLoader : MonoBehaviour
{
    public string playSceneName = "PlayScene"; 
    [SerializeField] private AudioSource uiAudioSource;

    public void OnNewGame()
    {
        PlayerPrefs.DeleteKey("SavedNodeName");
        PlayerPrefs.SetInt("IsNewGame", 1);
        PlayerPrefs.Save();

        PlayClickAndLoad();
    }

    public void OnLoadGame()
    {
        PlayerPrefs.SetInt("IsNewGame", 0);
        PlayerPrefs.Save();

        PlayClickAndLoad();
    }

    private void PlayClickAndLoad()
    {
        if (uiAudioSource != null && uiAudioSource.clip != null)
        {
            uiAudioSource.Play();
            StartCoroutine(LoadSceneAfterDelay(0.2f)); // ⏱️ fixed delay regardless of clip length
        }
        else
        {
            SceneManager.LoadScene(playSceneName);
        }
    }


    private IEnumerator LoadSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(playSceneName);
    }
}
