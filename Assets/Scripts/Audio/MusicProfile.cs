using UnityEngine;

[CreateAssetMenu(fileName = "MusicProfile", menuName = "Audio/MusicProfile", order = 1)]
public class MusicProfile : ScriptableObject
{
    [Header("Tracks")]
    public AudioClip normal;
    public AudioClip caution;
    public AudioClip alert;
}
