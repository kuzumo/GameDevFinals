using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessSlider : MonoBehaviour
{
    public Volume volume;
    public Slider brightnessSlider;

    private ColorAdjustments colorAdjust;

    void Start()
    {
        if (volume.profile.TryGet(out colorAdjust))
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }
    }

    void SetBrightness(float value)
    {
        colorAdjust.postExposure.value = value;
    }
}
