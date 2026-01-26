using UnityEngine;

public class MainMenu : SubMenu
{
    public void NewGame()
    {
        GameManager.NewGame();
    }

    public void LoadGame(GameSave gameSave)
    {
        GameManager.LoadGame(gameSave);
    }
}
