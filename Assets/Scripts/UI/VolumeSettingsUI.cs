using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider VolumeSlider;
    private const string MASTER_KEY = "MasterVolume";

    private void Start()
    {
        LoadAndApplySliders();
        VolumeSlider.onValueChanged.AddListener(OnMasterSliderChanged);
    }

    private void LoadAndApplySliders()
    {
        float masterVolume = PlayerPrefs.GetFloat(MASTER_KEY, 1.0f);
        VolumeSlider.value = masterVolume;
        ApplyAudioVolume(MASTER_KEY, masterVolume);
    }

    private void OnMasterSliderChanged(float value)
    {
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        ApplyAudioVolume(MASTER_KEY, value);
    }

    private void ApplyAudioVolume(string key, float value)
    {
        Debug.Log($"Applying {key}: {value}");
    }

    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
}