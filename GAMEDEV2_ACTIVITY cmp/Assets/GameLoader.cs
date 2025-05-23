using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
    public string playSceneName = "PlayScene"; // Set this to your gameplay scene name

    public void OnNewGame()
    {
        PlayerPrefs.DeleteKey("SavedNodeName"); // Clear previous save
        PlayerPrefs.SetInt("IsNewGame", 1);      // New game flag
        PlayerPrefs.Save();
        SceneManager.LoadScene(playSceneName);
    }

    public void OnLoadGame()
    {
        PlayerPrefs.SetInt("IsNewGame", 0);      // Load game flag
        PlayerPrefs.Save();
        SceneManager.LoadScene(playSceneName);
    }
}
