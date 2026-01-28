using UnityEngine;

[CreateAssetMenu(fileName = "MusicProfile", menuName = "Data/Music Profile", order = 1)]
public class MusicProfile : ScriptableObject
{
    [Header("Tracks")]
    public AudioClip normal;
    public AudioClip caution;
    public AudioClip alert;
}
