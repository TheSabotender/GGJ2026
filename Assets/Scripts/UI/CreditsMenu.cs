using UnityEngine;

public class CreditsMenu : SubMenu
{
    public void BackToMainMenu()
    {
        MenuManager.SetScreen(MenuManager.Screen.Main);
    }
}
