using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveDisplay : MonoBehaviour
{
    public TextMeshProUGUI saveNameText;
    public TextMeshProUGUI startDateTimeText;
    public TextMeshProUGUI playtime;
    public TextMeshProUGUI lastDateTimeText;
    public TextMeshProUGUI masksCollectedText;
    public Button selectButton;

    private GameSave save;
    private SaveLoadScreen saveLoadScreen;

    public void Setup(GameSave gameSave, SaveLoadScreen owner)
    {
        save = gameSave;
        saveLoadScreen = owner;

        if (saveNameText != null)
            saveNameText.text = gameSave?.SaveName ?? string.Empty;

        if (startDateTimeText != null)
            startDateTimeText.text = gameSave?.StartDateTime ?? string.Empty;

        if (playtime != null)
            playtime.text = gameSave != null ? gameSave.Playtime.ToString(@"dd\.hh\:mm\:ss") : string.Empty;

        if (lastDateTimeText != null)
            lastDateTimeText.text = gameSave?.LastSaveTime ?? string.Empty;

        if (masksCollectedText != null)
            masksCollectedText.text = gameSave != null ? gameSave.MasksCollected.ToString() : string.Empty;
    }

    public void OnSelect()
    {
        if (saveLoadScreen == null)
            return;

        saveLoadScreen.SelectSave(save);
    }
}
