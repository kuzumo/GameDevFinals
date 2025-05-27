using UnityEngine;

public class TypingSpeedManager : MonoBehaviour
{
    public static TypingSpeedManager Instance;

    public float TypingSpeed => PlayerPrefs.GetFloat("TypingSpeed", 0.2f); // default = 0.2

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTypingSpeed(float speed)
    {
        PlayerPrefs.SetFloat("TypingSpeed", speed);
        PlayerPrefs.Save();
    }
}
