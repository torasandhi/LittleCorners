using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    public float MasterVolume { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        
        LoadSettings();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
        Debug.Log("Volume: " + value);
    }

    private void LoadSettings()
    {
        MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
    }
}