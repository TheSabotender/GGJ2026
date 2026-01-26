using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [SerializeField]
    private PlayerBrain playerBrain = null;

    [SerializeField]
    private CharacterProfile startingMask;

    [SerializeField]
    private CharacterProfile[] allProfiles;

    private GameSave currentGameSave = null;

    public static PlayerBrain PlayerBrain => instance.playerBrain;

    public static GameSave CurrentGameSave => instance.currentGameSave;

    public static CharacterProfile[] AllProfiles => instance.allProfiles;

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
        instance.currentGameSave = new GameSave();
        instance.currentGameSave.MasksCollected = 1;
        instance.currentGameSave.Masks = new() { instance.startingMask.Guid };
        instance.currentGameSave.GameVersion = Application.version;
        instance.currentGameSave.StartDateTime = System.DateTime.Now.Ticks.ToString();

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
        instance.currentGameSave = gameSave;

        // TODO: Apply save data to the current game state.
    }
}
