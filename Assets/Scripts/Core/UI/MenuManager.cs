using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Device;
using static MenuManager;

public class MenuManager : MonoBehaviour
{
    public enum Screen
    {
        None,

        Main,
        Load,
        Settings,
        Credits,

        Pause,
        Save,
        Mask,
    }

    private static MenuManager _instance;

    [SerializeField]
    private Screen startingScreen = Screen.None;

    private List<SubMenu> screens = new List<SubMenu>();
    private static Screen currentScreen = Screen.None;
    private SubMenu current;
    

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;

        screens = GetComponentsInChildren<SubMenu>(true).ToList();
        foreach (var subMenu in screens)
            subMenu.gameObject.SetActive(false);

        SetScreen(startingScreen);
    }

    public static void SetScreen(Screen screen)
    {
        if (_instance == null || currentScreen == screen)
            return;
        _instance.StartCoroutine(_instance.TransitionScreen(screen));
    }

    private IEnumerator TransitionScreen(Screen screen)
    {
        if (current != null)
            yield return current.Hide();

        currentScreen = screen;
        current = screens.FirstOrDefault(s => s.screenType == screen);

        if (current != null)
            yield return current.Show();
    }
}
