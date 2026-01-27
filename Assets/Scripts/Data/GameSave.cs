using System;
using System.Collections.Generic;

[Serializable]
public class GameSave
{
    //Metadata about the save
    public string SaveName;
    public string StartDateTime;
    public string DateTime;
    public string GameVersion;

    //Player data
    public int MasksCollected;
    public List<string> Masks;
    public string CurrentMask;
}
