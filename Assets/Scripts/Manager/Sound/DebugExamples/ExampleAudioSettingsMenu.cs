using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example audio settings menu that hooks up UI sliders to the SoundManager.
/// Attach this to your settings menu canvas or panel.
/// </summary>
public class ExampleAudioSettingsMenu : MonoBehaviour
{
    [Header("UI Slider References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Optional: UI Sound")]
    [SerializeField] private SoundDataSO uiSounds;

    private void Start()
    {
        LoadVolumeSettings();
        SetupSliderListeners();
    }

    private void SetupSliderListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void LoadVolumeSettings()
    {
        // Load saved settings from PlayerPrefs
        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Apply to SoundManager
        SoundManager.Instance.SetMasterVolume(master);
        SoundManager.Instance.SetMusicVolume(music);
        SoundManager.Instance.SetSFXVolume(sfx);

        // Update UI sliders
        if (masterVolumeSlider != null) masterVolumeSlider.value = master;
        if (musicVolumeSlider != null) musicVolumeSlider.value = music;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfx;
    }

    private void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
        SaveVolumeSetting("MasterVolume", value);
        PlayUISound();
    }

    private void OnMusicVolumeChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
        SaveVolumeSetting("MusicVolume", value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
        SaveVolumeSetting("SFXVolume", value);
        PlayUISound();
    }

    private void SaveVolumeSetting(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    private void PlayUISound()
    {
        // Optional: Play a UI sound when slider changes
        if (uiSounds != null)
        {
            SoundManager.Instance.PlaySound(uiSounds, UISoundEvent.SliderMove);
        }
    }

    // Optional: Call this from a "Reset to Default" button
    public void ResetToDefaults()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 1f;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.7f;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 1f;
        }
    }
}
