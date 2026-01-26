using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private MainMenu menuPrefab;

    [SerializeField]
    private Transform menuParent;

    private MainMenu menuInstance;

    public MainMenu MenuInstance => menuInstance;

    private void Awake()
    {
        if (menuPrefab == null)
        {
            Debug.LogWarning("GameManager is missing a menu prefab reference.");
            return;
        }

        if (menuInstance == null)
        {
            menuInstance = Instantiate(menuPrefab, menuParent);
            menuInstance.SetGameManager(this);
        }
    }

    public void NewGame()
    {
        // TODO: Trigger intro cutscene or any new game setup here.
        Debug.Log("NewGame called - hook up intro cutscene here.");
    }

    public void LoadGame(GameSave gameSave)
    {
        if (gameSave == null)
        {
            Debug.LogWarning("LoadGame called with null GameSave.");
            return;
        }

        Debug.Log($"LoadGame called for save: {gameSave.SaveName}");
        // TODO: Apply save data to the current game state.
    }
}
