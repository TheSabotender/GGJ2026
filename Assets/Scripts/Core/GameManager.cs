using System;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum AlertState
    {
        Normal,
        Caution,
        Alert
    }
    
    private static GameManager instance = null;

    [SerializeField]
    private PlayerBrain playerBrain = null;

    [SerializeField]
    private CharacterProfile startingMask;
    public CharacterProfile[] testMasks;

    [SerializeField]
    private CharacterProfile[] allProfiles;

    [SerializeField]
    private float frontDepthZ = -1f;

    [SerializeField]
    private float backDepthZ = 0f;

    private GameSave currentGameSave = null;

    public static PlayerBrain PlayerBrain => instance.playerBrain;

    public static GameSave CurrentGameSave => instance?.currentGameSave;

    public static CharacterProfile[] AllProfiles => instance.allProfiles;

    public static AlertState CurrentAlertState => RegionManager.CurrentRegion.AlertState;

    public static float FrontDepthZ => instance.frontDepthZ;
    public static float BackDepthZ => instance.backDepthZ;

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad7))
            RegionManager.SetAlertState(AlertState.Normal);
        if (Input.GetKeyDown(KeyCode.Keypad8))
            RegionManager.SetAlertState(AlertState.Caution);
        if (Input.GetKeyDown(KeyCode.Keypad9))
            RegionManager.SetAlertState(AlertState.Alert);
    }

    public static void NewGame()
    {
        var newGame = new GameSave();        
        newGame.Masks = new() {
            new MaskState() { guid = instance.startingMask.Guid, status = MaskStatus.Compromised },
        };

        if (instance.testMasks != null && instance.testMasks.Length > 0)
        {
            foreach (var profile in instance.testMasks)
            {
                if (profile != null && profile.Guid != instance.startingMask.Guid)
                {
                    newGame.Masks.Add(new MaskState() { guid = profile.Guid, status = MaskStatus.Fresh });
                }
            }
        }

        newGame.MasksCollected = newGame.Masks.Count;
        newGame.GameVersion = Application.version;
        newGame.StartDateTime = System.DateTime.Now.Ticks.ToString();
        newGame.CurrentMask = 0;
        instance.currentGameSave = newGame;

        Debug.Log("NewGame called - hook up intro cutscene here.");

        PlayerBrain.SwapMask(newGame.Masks[newGame.CurrentMask], newGame.CurrentProfile, force: true);
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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(transform.position.x - 1f, transform.position.y, frontDepthZ),
                        new Vector3(transform.position.x + 1f, transform.position.y, frontDepthZ));

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(transform.position.x - 1f, transform.position.y, backDepthZ),
                        new Vector3(transform.position.x + 1f, transform.position.y, backDepthZ));
    }
}
