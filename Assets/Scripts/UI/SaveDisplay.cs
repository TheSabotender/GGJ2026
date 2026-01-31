using TMPro;
using UnityEngine;

public class SaveDisplay : MonoBehaviour
{
    public TMP_Text saveNameText;
    public TMP_Text startDateTimeText;
    public TMP_Text dateTimeText;
    public TMP_Text gameVersionText;
    public TMP_Text masksCollectedText;
    public TMP_Text currentMaskText;
    public TMP_Text masksCountText;

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

        if (dateTimeText != null)
            dateTimeText.text = gameSave?.DateTime ?? string.Empty;

        if (gameVersionText != null)
            gameVersionText.text = gameSave?.GameVersion ?? string.Empty;

        if (masksCollectedText != null)
            masksCollectedText.text = gameSave != null ? gameSave.MasksCollected.ToString() : string.Empty;

        if (currentMaskText != null)
            currentMaskText.text = gameSave != null ? gameSave.CurrentMask.ToString() : string.Empty;

        if (masksCountText != null)
            masksCountText.text = gameSave?.Masks != null ? gameSave.Masks.Count.ToString() : string.Empty;
    }

    public void OnSelect()
    {
        if (saveLoadScreen == null)
            return;

        saveLoadScreen.SelectSave(save);
    }
}
