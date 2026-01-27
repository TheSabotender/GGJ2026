using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LocalizationEntry
{
    public string Key;
    public string Value;
}

[Serializable]
public class LocalizationTable
{
    public List<LocalizationEntry> Entries = new List<LocalizationEntry>();
}

public static class LocalizationManager
{
    private const string ResourcesFolder = "Languages";
    private static readonly Dictionary<string, string> Localization = new Dictionary<string, string>();
    private static bool isLoaded;
    private static Language loadedLanguage = Language.English;

    public static bool TryGetValue(string key, out string value)
    {
        EnsureLoaded();
        if (string.IsNullOrEmpty(key))
        {
            value = string.Empty;
            return false;
        }

        return Localization.TryGetValue(key, out value);
    }

    public static bool ContainsKey(string key)
    {
        EnsureLoaded();
        return !string.IsNullOrEmpty(key) && Localization.ContainsKey(key);
    }

    private static void EnsureLoaded()
    {
        var desiredLanguage = GetDesiredLanguage();
        if (isLoaded && loadedLanguage == desiredLanguage)
        {
            return;
        }

        LoadLanguage(desiredLanguage);
    }

    private static Language GetDesiredLanguage()
    {
        if (!Application.isPlaying)
        {
            return Language.English;
        }

        var settings = SettingsManager.Load();
        return settings != null ? settings.Language : Language.English;
    }

    private static void LoadLanguage(Language language)
    {
        var textAsset = LoadLanguageAsset(language);
        if (textAsset == null && language != Language.English)
        {
            language = Language.English;
            textAsset = LoadLanguageAsset(language);
        }

        Localization.Clear();
        if (textAsset != null)
        {
            var table = JsonUtility.FromJson<LocalizationTable>(textAsset.text);
            if (table != null && table.Entries != null)
            {
                foreach (var entry in table.Entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.Key))
                    {
                        continue;
                    }

                    Localization[entry.Key] = entry.Value ?? string.Empty;
                }
            }
        }

        loadedLanguage = language;
        isLoaded = true;
    }

    private static TextAsset LoadLanguageAsset(Language language)
    {
        var languageName = language.ToString();
        var resourcePath = string.Format("{0}/{1}", ResourcesFolder, languageName);
        return Resources.Load<TextAsset>(resourcePath);
    }
}
