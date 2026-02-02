using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField]
    private InputActionReference skipAction = null;

    [SerializeField]
    private string mainSceneName = "Main";

    [SerializeField]
    private float animationDuration = 1.0f;

    [SerializeField]
    private Image skipGraphic;

    [SerializeField]
    private List<GameObject> essentialPrefabs = new List<GameObject>();

    private void Start()
    {
        StartCoroutine(LoadEssentialsAndMainScene());
    }

    private System.Collections.IEnumerator LoadEssentialsAndMainScene()
    {
        foreach (GameObject prefab in essentialPrefabs)
        {
            if (prefab == null)
            {
                continue;
            }

            GameObject instance = Instantiate(prefab);
            DontDestroyOnLoad(instance);
            yield return null;
        }

        var t = 0f;
        var skipHoldTime = 0f;
        while (t <= animationDuration)
        {
            if (skipAction?.action != null && skipAction.action.IsPressed())
            {
                skipHoldTime += Time.deltaTime;
                if (skipGraphic != null)
                    skipGraphic.fillAmount = Mathf.Clamp01(skipHoldTime / animationDuration);

                if (skipHoldTime >= animationDuration)
                    break;
            }
            else
            {
                skipHoldTime = 0f;
                if (skipGraphic != null)
                    skipGraphic.fillAmount = 0f;
            }

            t += Time.deltaTime;
            yield return null;
        }

        LoadMainScene();
    }

    private void LoadMainScene()
    {
        if (string.IsNullOrWhiteSpace(mainSceneName))
        {
            Debug.LogWarning("Bootstrapper main scene name is not set.");
            return;
        }

        if (SceneManager.GetActiveScene().name == mainSceneName)
        {
            return;
        }

        SceneManager.LoadScene(mainSceneName);
    }
}
