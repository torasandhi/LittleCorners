using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [SerializeField] private AudioMixer bgmMixer;
    [SerializeField] private Slider volumeSlider;
    
    public float MasterVolume { get; private set; }

    private const string VolumeKey = "MasterVolume";
    private const string MixerParameter = "BGMVolume";

    private void Awake()
    {
        Instance = this;
        MasterVolume = PlayerPrefs.GetFloat(VolumeKey, 1f);
    }

    private void Start()
    {
        // Update the slider without invoking its event.
        if (volumeSlider != null)
            volumeSlider.SetValueWithoutNotify(MasterVolume);

        ApplyVolume();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        ApplyVolume();

        PlayerPrefs.SetFloat(VolumeKey, MasterVolume);
        PlayerPrefs.Save();
    }

    private void ApplyVolume()
    {
        if (bgmMixer == null)
        {
            Debug.LogWarning("BGM Mixer has not been assigned.");
            return;
        }

        float decibels = MasterVolume <= 0.0001f
            ? -80f
            : Mathf.Log10(MasterVolume) * 20f;

        if (!bgmMixer.SetFloat(MixerParameter, decibels))
        {
            Debug.LogWarning(
                $"Mixer parameter '{MixerParameter}' was not found. " +
                "Check that it is exposed and named correctly."
            );
        }
    }
}