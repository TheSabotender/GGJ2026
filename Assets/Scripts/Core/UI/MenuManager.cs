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

    [SerializeField]
    private Screen startingScreen = Screen.None;

    [SerializeField]
    private List<SubMenu> screens = new List<SubMenu>();

    private Screen currentScreen = Screen.None;
    private SubMenu current;
    

    private void Awake()
    {
        foreach (var subMenu in screens)
            subMenu.gameObject.SetActive(false);

        SetScreen(startingScreen);
    }

    public void SetScreen(Screen screen)
    {
        if (currentScreen == screen)
            return;
        StartCoroutine(TransitionScreen(screen));
    }

    public IEnumerator TransitionScreen(Screen screen)
    {
        if (current != null)
            yield return current.Hide();

        currentScreen = screen;
        current = screens.FirstOrDefault(s => s.screenType == screen);

        if (current != null)
            yield return current.Show();
    }
}
