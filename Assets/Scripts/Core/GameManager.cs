using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [SerializeField]
    private PlayerBrain playerBrain = null;

    public static PlayerBrain PlayerBrain => instance.playerBrain;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public static void NewGame()
    {
        // TODO: Trigger intro cutscene or any new game setup here.
        Debug.Log("NewGame called - hook up intro cutscene here.");
    }

    public static void LoadGame(GameSave gameSave)
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
