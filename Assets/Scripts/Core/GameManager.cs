using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    [SerializeField]
    private CharacterProfile startingMask;
    public CharacterProfile[] testMasks;

    private GameSave currentGameSave = null;

    private static CharacterProfile[] allProfiles;

    public static GameSave CurrentGameSave => instance?.currentGameSave;

    public static event Action<GameSave> GameLoaded;

    public static CharacterProfile[] AllProfiles
    {
        get
        {
            if (allProfiles == null || allProfiles.Length == 0)
            {
                allProfiles = Resources.LoadAll<CharacterProfile>("CharacterProfiles");
            }
            return allProfiles;
        }
    }

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

        instance.StartCoroutine(instance.LoadGameRoutine(newGame));

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
        
        instance.StartCoroutine(instance.LoadGameRoutine(gameSave));

    }

    private IEnumerator LoadGameRoutine(GameSave gameSave)
    {
        MenuManager.SetScreen(MenuManager.Screen.None);
        yield return null;

        var sceneLoad = SceneManager.LoadSceneAsync("Game");
        if (sceneLoad != null)
        {
            while (!sceneLoad.isDone)
                yield return null;
        }

        GameLoaded?.Invoke(gameSave);
    }
}
