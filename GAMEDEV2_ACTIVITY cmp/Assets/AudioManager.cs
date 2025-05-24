using UnityEngine;
using UnityEngine.UI;
using TMPro; // If using TextMeshPro

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public Button soundToggleButton;
    public TextMeshProUGUI soundButtonText; // Or use UnityEngine.UI.Text if using classic Text

    public bool isMuted = false;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        isMuted = PlayerPrefs.GetInt("SoundMuted", 0) == 1;
        AudioListener.volume = isMuted ? 0 : 1;

        UpdateSoundButtonText();
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0 : 1;
        PlayerPrefs.SetInt("SoundMuted", isMuted ? 1 : 0);

        UpdateSoundButtonText();
    }

    private void UpdateSoundButtonText()
    {
        if (soundButtonText != null)
        {
            soundButtonText.text = isMuted ? "OFF" : "ON";
            soundButtonText.color = isMuted ? Color.red : Color.green;
        }
    }

}
