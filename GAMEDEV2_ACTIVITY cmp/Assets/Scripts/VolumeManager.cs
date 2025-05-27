using UnityEngine;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioSource musicSource;

    void Awake()
    {
        // Automatically assign the AudioSource if it's not set
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        // Initialize PlayerPrefs if needed
        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1f);
        }
    }

    void Start()
    {
        Load();
        ApplyVolume();
    }

    public void ChangeVolume()
    {
        ApplyVolume();
        Save();
    }

    private void Load()
    {
        float savedVolume = PlayerPrefs.GetFloat("musicVolume");
        volumeSlider.value = savedVolume;
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", volumeSlider.value);
    }

    private void ApplyVolume()
    {
        if (musicSource != null)
        {
            musicSource.volume = volumeSlider.value;
        }
    }

    void OnEnable()
    {
        Load();
        ApplyVolume();
    }
}
