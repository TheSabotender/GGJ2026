using UnityEngine;

public class GameManager : MonoBehaviour
{
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
