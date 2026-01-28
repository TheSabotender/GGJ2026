using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class GameSave
{
    //Metadata about the save
    public string SaveName;
    public string StartDateTime;
    public string DateTime;
    public string GameVersion;

    //Player data
    public List<MaskState> Masks;
    public int CurrentMask;
    public int MasksCollected;

    public CharacterProfile CurrentProfile => GameManager.AllProfiles.FirstOrDefault(p => p.Guid == Masks[CurrentMask].guid);
}

[Serializable]
public class MaskState
{
    public string guid;
    public MaskStatus status;
}

public enum MaskStatus
{
    Fresh,
    Suspicious,
    Compromised
}