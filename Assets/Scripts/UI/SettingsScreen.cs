using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : SubMenu
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;

    private GameSettings settings;

    private void Awake()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
    }

    private void OnEnable()
    {
        settings = SettingsManager.Load();

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(settings.MasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(settings.MusicVolume);

        if (soundVolumeSlider != null)
            soundVolumeSlider.SetValueWithoutNotify(settings.SoundVolume);
    }

    public void OnBack()
    {
        MenuManager.SetScreen(GameManager.CurrentGameSave != null
            ? MenuManager.Screen.Pause
            : MenuManager.Screen.Main);
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
