using System.Linq;
using UnityEngine;
using static GameManager;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager instance = null;


    [SerializeField]
    private float frontDepthZ = -1f;

    [SerializeField]
    private float backDepthZ = 0f;

    [SerializeField]
    private PlayerBrain playerBrain = null;

    private bool isGameLoaded;

    public static PlayerBrain PlayerBrain => instance?.playerBrain;
    public static float FrontDepthZ => instance.frontDepthZ;
    public static float BackDepthZ => instance.backDepthZ;
    public static AlertState CurrentAlertState => RegionManager.CurrentRegion.AlertState;
    public static bool IsGameLoaded => instance.isGameLoaded;

    private void Start()
    {
        instance = this;
        RegionManager.UpdateCurrentRegion();
        isGameLoaded = GameManager.CurrentGameSave != null;
        if (isGameLoaded)
            HandleGameLoaded(GameManager.CurrentGameSave);

        GameManager.GameLoaded += HandleGameLoaded;
    }

    private void OnDestroy()
    {
        GameManager.GameLoaded -= HandleGameLoaded;
    }

    private void HandleGameLoaded(GameSave save)
    {
        if (MenuManager.CurrentScreen != MenuManager.Screen.None)
            return;

        RegionManager.UpdateCurrentRegion();
        isGameLoaded = GameManager.CurrentGameSave != null;
        SceneLoaded(save);
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

    void SceneLoaded(GameSave save)
    {
        var currentGame = GameManager.CurrentGameSave;

        if (PlayerBrain != null && currentGame.Masks != null && currentGame.Masks.Count > 0)
        {
            var currentMask = GameManager.CurrentGameSave.Masks[currentGame.CurrentMask];
            playerBrain.SwapMask(currentMask, currentGame.CurrentProfile, force: true);
        }

        if (playerBrain != null && !string.IsNullOrWhiteSpace(currentGame.SavePointGuid))
        {
            var savePoints = FindObjectsByType<SavePoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            var matchingSavePoint = savePoints.FirstOrDefault(point => point.Guid == currentGame.SavePointGuid);
            if (matchingSavePoint != null)
            {
                playerBrain.transform.position = matchingSavePoint.transform.position;
            }
        }
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