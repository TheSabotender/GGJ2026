using System;
using UnityEngine;

[Serializable]
public class GameSettings
{
    public FullScreenMode RenderMode = FullScreenMode.MaximizedWindow;
    public Language Language = Language.English;
    public float MasterVolume = 1f;
    public float MusicVolume = 1f;
    public float SoundVolume = 1f;
}

public enum Language
{
    English,
    Norwegian,
    Swedish,
    Danish,
    Finish,
    Spanish,
    French,
    German,
    Chinese,
    Japanese,
    Korean,
}