using UnityEngine;
using UnityEngine.UI;

public class MainMenu : SubMenu
{
    public Button continueButton;
    public Button loadButton;

    private void OnEnable()
    {
        var hasContinue = SaveManager.HasLastSave();
        continueButton.interactable = hasContinue;

        var hasSaves = SaveManager.LoadAll().Length > 0;
        loadButton.interactable = hasSaves;
    }

    public void ContinueGame()
    {
        var save = SaveManager.GetLastSave();
        if (save == null)
            return;

        MenuManager.SetScreen(MenuManager.Screen.None);
        GameManager.LoadGame(save);
    }

    public void NewGame()
    {
        MenuManager.SetScreen(MenuManager.Screen.None);
        GameManager.NewGame();
    }

    public void LoadGame()
    {
        MenuManager.SetScreen(MenuManager.Screen.Load);
    }

    public void Settings()
    {
        MenuManager.SetScreen(MenuManager.Screen.Settings);
    }

    public void Credits()
    {
        MenuManager.SetScreen(MenuManager.Screen.Credits);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
