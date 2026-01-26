using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private MenuScreen startingScreen = MenuScreen.Main;

    [SerializeField]
    private List<SubMenu> subMenuPrefabs = new List<SubMenu>();

    [SerializeField]
    private Transform subMenuParent;

    private readonly Dictionary<MenuScreen, SubMenu> subMenus = new Dictionary<MenuScreen, SubMenu>();
    private GameManager gameManager;
    private MenuScreen currentScreen = MenuScreen.None;

    private void Awake()
    {
        InitializeSubMenus();
        SetScreen(startingScreen);
    }

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    public void SetScreen(MenuScreen screen)
    {
        if (currentScreen == screen)
        {
            return;
        }

        if (subMenus.TryGetValue(currentScreen, out SubMenu currentMenu))
        {
            currentMenu.Hide();
        }

        currentScreen = screen;

        if (subMenus.TryGetValue(currentScreen, out SubMenu nextMenu))
        {
            nextMenu.Show();
        }
    }

    public void NewGame()
    {
        if (gameManager == null)
        {
            Debug.LogWarning("MainMenu cannot start a new game without a GameManager.");
            return;
        }

        gameManager.NewGame();
    }

    public void LoadGame(GameSave gameSave)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("MainMenu cannot load a game without a GameManager.");
            return;
        }

        gameManager.LoadGame(gameSave);
    }

    private void InitializeSubMenus()
    {
        foreach (SubMenu prefab in subMenuPrefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            if (subMenus.ContainsKey(prefab.Screen))
            {
                Debug.LogWarning($"Duplicate submenu for {prefab.Screen} on {name}.");
                continue;
            }

            SubMenu instance = Instantiate(prefab, subMenuParent);
            instance.Hide();
            subMenus.Add(instance.Screen, instance);
        }
    }
}
