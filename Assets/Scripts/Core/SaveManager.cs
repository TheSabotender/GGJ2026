using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class SaveManager
{
    private const string SaveFolderName = "saves";
    private const string SaveExtension = ".save";
    private const string BinaryPrefix = "BASE64:";
    private const string LastSaveKey = "SaveManager.LastSaveName";

    public static string SaveFolderPath => Path.Combine(Application.persistentDataPath, SaveFolderName);

    public static void Save(GameSave save, bool humanReadable)
    {
        if (save == null)
        {
            throw new ArgumentNullException(nameof(save));
        }

        Directory.CreateDirectory(SaveFolderPath);

        var fileName = GetFileName(save.SaveName);
        var filePath = Path.Combine(SaveFolderPath, fileName);
        if (humanReadable)
        {
            var json = JsonUtility.ToJson(save, true);
            File.WriteAllText(filePath, json, Encoding.UTF8);
            StoreLastSaveName(fileName);
            return;
        }

        var payload = SerializeToBase64(save);
        File.WriteAllText(filePath, $"{BinaryPrefix}{payload}", Encoding.UTF8);
        StoreLastSaveName(fileName);
    }

    public static GameSave Load(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        var filePath = GetFilePath(fileName);

        return LoadFromFilePath(filePath);
    }

    public static GameSave GetLastSave()
    {
        var lastSaveName = PlayerPrefs.GetString(LastSaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(lastSaveName))
        {
            return null;
        }

        return Load(lastSaveName);
    }

    public static bool HasLastSave()
    {
        if (!PlayerPrefs.HasKey(LastSaveKey))
        {
            return false;
        }

        var lastSaveName = PlayerPrefs.GetString(LastSaveKey, string.Empty);
        if (string.IsNullOrWhiteSpace(lastSaveName))
        {
            return false;
        }

        var filePath = GetFilePath(lastSaveName);
        return File.Exists(filePath);
    }

    public static void DeleteSave(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
        }

        var filePath = GetFilePath(fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var lastSaveName = PlayerPrefs.GetString(LastSaveKey, string.Empty);
        var normalizedName = Path.IsPathRooted(fileName) ? Path.GetFileName(fileName) : fileName;
        if (!string.IsNullOrWhiteSpace(lastSaveName)
            && string.Equals(lastSaveName, normalizedName, StringComparison.OrdinalIgnoreCase))
        {
            PlayerPrefs.DeleteKey(LastSaveKey);
            PlayerPrefs.Save();
        }
    }

    public static GameSave[] LoadAll()
    {
        if (!Directory.Exists(SaveFolderPath))
        {
            return Array.Empty<GameSave>();
        }

        var files = Directory.GetFiles(SaveFolderPath, $"*{SaveExtension}", SearchOption.TopDirectoryOnly);

        return files.Select(LoadFromFilePath)
            .Where(save => save != null)
            .ToArray();
    }

    private static GameSave LoadFromFilePath(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return null;
        }

        var text = File.ReadAllText(filePath, Encoding.UTF8);
        if (text.StartsWith(BinaryPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var payload = text.Substring(BinaryPrefix.Length);
            return DeserializeFromBase64(payload);
        }

        return JsonUtility.FromJson<GameSave>(text);
    }

    private static string GetFilePath(string fileName)
    {
        return Path.IsPathRooted(fileName)
            ? fileName
            : Path.Combine(SaveFolderPath, fileName);
    }

    private static string GetFileName(string saveName)
    {
        var safeName = string.IsNullOrWhiteSpace(saveName)
            ? "save"
            : string.Concat(saveName.Split(Path.GetInvalidFileNameChars()));

        return $"{safeName}{SaveExtension}";
    }

    private static string SerializeToBase64(GameSave save)
    {
        using var memoryStream = new MemoryStream();
        using (var writer = new BinaryWriter(memoryStream, Encoding.UTF8, true))
        {
            writer.Write(save.SaveName ?? string.Empty);
            writer.Write(save.DateTime ?? string.Empty);
            writer.Write(save.PlayTime);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private static void StoreLastSaveName(string fileName)
    {
        PlayerPrefs.SetString(LastSaveKey, fileName);
        PlayerPrefs.Save();
    }

    private static GameSave DeserializeFromBase64(string payload)
    {
        var data = Convert.FromBase64String(payload);
        using var memoryStream = new MemoryStream(data);
        using var reader = new BinaryReader(memoryStream, Encoding.UTF8, true);

        return new GameSave
        {
            SaveName = reader.ReadString(),
            DateTime = reader.ReadString(),
            PlayTime = reader.ReadSingle()
        };
    }
}
