using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TypingSpeedSlider : MonoBehaviour
{
    public Slider speedSlider; // assign in Inspector
    public TMP_Text speedLabel; // optional: display speed as text

    private const float maxTypingSpeed = 0.05f; // slowest
    private const float minTypingSpeed = 0.01f; // fastest

    void Start()
    {
        // Load saved speed and convert to slider value
        float currentSpeed = PlayerPrefs.GetFloat("TypingSpeed", 0.02f);
        speedSlider.value = Mathf.InverseLerp(maxTypingSpeed, minTypingSpeed, currentSpeed);
        UpdateLabel(currentSpeed);

        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
    }

    public void OnSpeedChanged(float sliderValue)
    {
        // Invert: sliderValue 0 = slow, 1 = fast
        float typingSpeed = Mathf.Lerp(maxTypingSpeed, minTypingSpeed, sliderValue);
        PlayerPrefs.SetFloat("TypingSpeed", typingSpeed);
        PlayerPrefs.Save();

        UpdateLabel(typingSpeed);
    }

    private void UpdateLabel(float speed)
    {
        if (speedLabel != null)
            speedLabel.text = $"Speed: {speed:0.00}s";
    }
}
