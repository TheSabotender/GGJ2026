using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Device;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private SubMenu startingScreen;

    private SubMenu currentScreen;
    private List<SubMenu> loadedScreens = new List<SubMenu>();

    private void Awake()
    {
        SetScreen(startingScreen);
    }

    public void SetScreen(SubMenu screen)
    {
        // Check if the requested screen is already loaded
        var instance = loadedScreens.Find(s => s.name == screen.name);

        // If the requested screen is already active, do nothing
        if (instance != null && instance == screen)
            return;

        // Load the screen if it hasn't been loaded yet
        if (instance == null)
        {
            instance = Instantiate(screen, transform);
            instance.name = screen.name;
            loadedScreens.Add(instance);
        }

        StartCoroutine(TransitionScreen(instance));
    }

    public IEnumerator TransitionScreen(SubMenu newScreen)
    {
        // Hide the current screen
        if (currentScreen != null)
            yield return currentScreen.Hide();

        // Set the current screen and show it
        currentScreen = newScreen;
        if (currentScreen != null)
            yield return currentScreen.Show();
    }
}
