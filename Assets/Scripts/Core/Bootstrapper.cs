using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField]
    private string mainSceneName = "Main";

    public float animationDuration = 1.0f;

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

        yield return new WaitForSeconds(animationDuration);

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
