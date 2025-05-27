using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadButtonHandler : MonoBehaviour
{
    public string gameplaySceneName = "GameScene"; // Set this in Inspector

    public void OnLoadButtonClicked()
    {
        PlayerPrefs.SetInt("IsNewGame", 0); // Tell the game this is a Load Game
        PlayerPrefs.Save(); // Ensure the flag is saved before scene changes
        SceneManager.LoadScene(gameplaySceneName);
    }
}
