using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [SerializeField]
    private PlayerBrain playerBrain = null;

    [SerializeField]
    private CharacterProfile startingMask;
    public CharacterProfile testMask;

    [SerializeField]
    private CharacterProfile[] allProfiles;

    private GameSave currentGameSave = null;

    public static PlayerBrain PlayerBrain => instance.playerBrain;

    public static GameSave CurrentGameSave => instance?.currentGameSave;

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
        var newGame = new GameSave();        
        newGame.Masks = new() {
            instance.startingMask.Guid,
            instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,instance.testMask.Guid,
        };
        newGame.MasksCollected = newGame.Masks.Count;
        newGame.GameVersion = Application.version;
        newGame.StartDateTime = System.DateTime.Now.Ticks.ToString();
        newGame.CurrentMask = instance.startingMask.Guid;

        instance.currentGameSave = newGame;

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
