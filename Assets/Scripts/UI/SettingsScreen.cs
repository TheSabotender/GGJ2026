using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : SubMenu
{
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider soundVolumeSlider;
    public TMPro.TMP_Dropdown renderModeDropdown;
    public TMPro.TMP_Dropdown languageDropdown;

    private GameSettings settings;

    private void Awake()
    {
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);

        if (renderModeDropdown != null)
        {
            PopulateDropdown<FullScreenMode>(renderModeDropdown);
            renderModeDropdown.onValueChanged.AddListener(OnRenderModeChanged);
        }

        if (languageDropdown != null)
        {
            PopulateDropdown<Language>(languageDropdown);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
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

        if (renderModeDropdown != null)
        {
            var renderModes = (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode));
            var index = Array.IndexOf(renderModes, settings.RenderMode);
            if (index >= 0)
                renderModeDropdown.SetValueWithoutNotify(index);
        }

        if (languageDropdown != null)
        {
            var languages = (Language[])Enum.GetValues(typeof(Language));
            var index = Array.IndexOf(languages, settings.Language);
            if (index >= 0)
                languageDropdown.SetValueWithoutNotify(index);
        }
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
        SettingsManager.NotifyMasterVolumeChanged(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        EnsureSettingsLoaded();
        settings.MusicVolume = value;
        SettingsManager.Save(settings);
        SettingsManager.NotifyMusicVolumeChanged(value);
    }

    public void OnSoundVolumeChanged(float value)
    {
        EnsureSettingsLoaded();
        settings.SoundVolume = value;
        SettingsManager.Save(settings);
        SettingsManager.NotifySoundVolumeChanged(value);
    }

    public void OnRenderModeChanged(int value)
    {
        EnsureSettingsLoaded();
        var renderModes = (FullScreenMode[])Enum.GetValues(typeof(FullScreenMode));
        if (value < 0 || value >= renderModes.Length)
            return;

        settings.RenderMode = renderModes[value];
        SettingsManager.Save(settings);
    }

    public void OnLanguageChanged(int value)
    {
        EnsureSettingsLoaded();
        var languages = (Language[])Enum.GetValues(typeof(Language));
        if (value < 0 || value >= languages.Length)
            return;

        settings.Language = languages[value];
        SettingsManager.Save(settings);
        LocalizationManager.ReloadLanguage();
    }

    private void EnsureSettingsLoaded()
    {
        if (settings == null)
        {
            settings = SettingsManager.Load();
        }
    }

    private void PopulateDropdown<TEnum>(TMPro.TMP_Dropdown dropdown) where TEnum : Enum
    {
        dropdown.ClearOptions();
        var options = new List<TMPro.TMP_Dropdown.OptionData>();
        foreach (var value in Enum.GetValues(typeof(TEnum)))
        {
            options.Add(new TMPro.TMP_Dropdown.OptionData(FormatEnumLabel(value.ToString())));
        }

        dropdown.AddOptions(options);
    }

    private static string FormatEnumLabel(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var builder = new StringBuilder();
        builder.Append(value[0]);

        for (int i = 1; i < value.Length; i++)
        {
            char current = value[i];
            char previous = value[i - 1];

            if (char.IsUpper(current) && !char.IsUpper(previous))
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }
}
