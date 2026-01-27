using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class SettingsManager
{
    private const string SettingsFileName = "settings.json";
    private static GameSettings cachedSettings;

    private static string SettingsFilePath => Path.Combine(Application.persistentDataPath, SettingsFileName);

    public static GameSettings Load()
    {
        if (cachedSettings != null)
        {
            return cachedSettings;
        }

        if (!File.Exists(SettingsFilePath))
        {
            cachedSettings = CreateDefaultSettings();
            return cachedSettings;
        }

        var json = File.ReadAllText(SettingsFilePath, Encoding.UTF8);
        var settings = JsonUtility.FromJson<GameSettings>(json);
        if (settings == null)
        {
            settings = CreateDefaultSettings();
        }

        cachedSettings = settings;
        return cachedSettings;
    }

    public static void Save(GameSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        cachedSettings = settings;
        var json = JsonUtility.ToJson(settings, true);
        File.WriteAllText(SettingsFilePath, json, Encoding.UTF8);
    }

    private static GameSettings CreateDefaultSettings()
    {
        return new GameSettings();
    }
}
