using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : SubMenu
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;

    private GameSettings settings;

    private void OnEnable()
    {
        settings = SettingsManager.Load();

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(settings.MusicVolume);
        }

        if (soundVolumeSlider != null)
        {
            soundVolumeSlider.SetValueWithoutNotify(settings.SoundVolume);
        }
    }

    public void OnMasterVolumeChanged(float value)
    {
        EnsureSettingsLoaded();
        settings.MasterVolume = value;
        SettingsManager.Save(settings);
    }

    public void OnMusicVolumeChanged(float value)
    {
        EnsureSettingsLoaded();
        settings.MusicVolume = value;
        SettingsManager.Save(settings);
    }

    public void OnSoundVolumeChanged(float value)
    {
        EnsureSettingsLoaded();
        settings.SoundVolume = value;
        SettingsManager.Save(settings);
    }

    private void EnsureSettingsLoaded()
    {
        if (settings == null)
        {
            settings = SettingsManager.Load();
        }
    }
}
