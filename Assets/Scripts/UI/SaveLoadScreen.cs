using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadScreen : SubMenu
{
    public enum SaveLoadMode
    {
        AllowSave,
        AllowLoad,
        AllowBoth
    }

    public SaveLoadMode mode = SaveLoadMode.AllowBoth;

    public SaveDisplay saveDisplayPrefab;
    public Transform saveContainer;
    public SaveDisplay selectedSaveDisplay;

    public Button saveButton;
    public Button loadButton;

    private readonly List<SaveDisplay> displays = new List<SaveDisplay>();
    private GameSave selectedSave;
    private GameSave newSaveEntry;

    private void OnEnable()
    {
        PopulateSaves();
        UpdateButtons();
    }

    public void SelectSave(GameSave save)
    {
        selectedSave = save;
        selectedSaveDisplay.Setup(save, null);
    }

    public void OnSavePressed()
    {
        if (mode == SaveLoadMode.AllowLoad)
            return;

        var currentSave = GameManager.CurrentGameSave;
        if (currentSave == null)
            return;

        var isNewSave = selectedSave == null || selectedSave == newSaveEntry;
        if (isNewSave)
        {
            currentSave.SaveName = $"Save {DateTime.Now:yyyy-MM-dd HHmmss}";
            currentSave.StartDateTime ??= DateTime.Now.ToString("G");
        }
        else
        {
            currentSave.SaveName = selectedSave.SaveName;
        }

        currentSave.LastSaveTime = DateTime.Now.ToString("G");
        SaveManager.Save(currentSave, humanReadable: true);
        PopulateSaves();
    }

    public void OnLoadPressed()
    {
        if (mode == SaveLoadMode.AllowSave)
            return;

        if (selectedSave == null || selectedSave == newSaveEntry)
            return;

        GameManager.LoadGame(selectedSave);
        MenuManager.SetScreen(MenuManager.Screen.None);
    }

    public void OnDeletePressed()
    {
        if (selectedSave == null || selectedSave == newSaveEntry)
            return;

        var fileName = GetSaveFileName(selectedSave.SaveName);
        SaveManager.DeleteSave(fileName);
        PopulateSaves();
    }

    public void OnBackPressed()
    {
        MenuManager.SetScreen(GameManager.CurrentGameSave != null
            ? MenuManager.Screen.Pause
            : MenuManager.Screen.Main);
    }

    private void PopulateSaves()
    {
        if (saveContainer == null || saveDisplayPrefab == null)
            return;

        foreach (var display in displays)
        {
            if (display != null)
                Destroy(display.gameObject);
        }
        displays.Clear();

        selectedSave = null;
        newSaveEntry = null;

        var saves = new List<GameSave>(SaveManager.LoadAll());
        if (mode == SaveLoadMode.AllowSave || mode == SaveLoadMode.AllowBoth)
        {
            newSaveEntry = new GameSave
            {
                SaveName = "New Save"
            };
            saves.Insert(0, newSaveEntry);
        }

        foreach (var save in saves)
        {
            var display = Instantiate(saveDisplayPrefab, saveContainer);
            display.Setup(save, this);
            displays.Add(display);
        }
    }

    private void UpdateButtons()
    {
        var allowSave = mode == SaveLoadMode.AllowSave || mode == SaveLoadMode.AllowBoth;
        var allowLoad = mode == SaveLoadMode.AllowLoad || mode == SaveLoadMode.AllowBoth;

        if (saveButton != null)
            saveButton.gameObject.SetActive(allowSave);

        if (loadButton != null)
            loadButton.gameObject.SetActive(allowLoad);
    }

    private static string GetSaveFileName(string saveName)
    {
        var safeName = string.IsNullOrWhiteSpace(saveName)
            ? "save"
            : string.Concat(saveName.Split(Path.GetInvalidFileNameChars()));

        return $"{safeName}.save";
    }
}
