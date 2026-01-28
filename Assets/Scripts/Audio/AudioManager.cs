using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class MusicSource
    {
        public AudioSource source;
        public bool isNeeded;
    }

    private static AudioManager instance;

    [SerializeField]
    private float positionUpdateThreshold = 0.5f;

    [SerializeField]
    private AudioSource musicSourcePrefab;

    [SerializeField]
    private float crossfadeSpeed;

    private readonly List<AudioSource> cachedAudioSources = new();
    private PlayerBrain playerBrain;
    private Vector3 lastPlayerPosition;
    private MusicProfile currentProfile;
    private List<MusicSource> musicSources = new();

    private void Awake()
    {
        instance = this;
        RefreshAudioSources();

        if (musicSources == null)
            musicSources = new();
    }

    private void Start()
    {
        playerBrain = GameManager.PlayerBrain;
        if (playerBrain != null)
        {
            lastPlayerPosition = playerBrain.transform.position;
            UpdateAudioSources();
        }
        GameManager.AlertStateChanged += OnAlertStateChanged;
    }

    private void Update()
    {
        UpdateMusic();

        if (playerBrain == null)
        {
            playerBrain = GameManager.PlayerBrain;
            if (playerBrain == null)
            {
                return;
            }

            lastPlayerPosition = playerBrain.transform.position;
            UpdateAudioSources();
            return;
        }

        var currentPosition = playerBrain.transform.position;
        if ((currentPosition - lastPlayerPosition).sqrMagnitude < positionUpdateThreshold * positionUpdateThreshold)
        {
            return;
        }

        lastPlayerPosition = currentPosition;
        UpdateAudioSources();
    }

    public void RefreshAudioSources()
    {
        cachedAudioSources.Clear();
        cachedAudioSources.AddRange(FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private void UpdateAudioSources()
    {
        if (playerBrain == null)
        {
            return;
        }

        var playerPosition = playerBrain.transform.position;
        foreach (var audioSource in cachedAudioSources)
        {
            if (audioSource == null)
            {
                continue;
            }

            if (audioSource.spatialBlend <= 0f)
            {
                continue;
            }

            var distance = Vector3.Distance(playerPosition, audioSource.transform.position);
            var isInRange = distance <= audioSource.maxDistance;
            if (audioSource.enabled != isInRange)
            {
                audioSource.enabled = isInRange;
            }
        }
    }

    public static void SetMusicProfile(MusicProfile newProfile)
    {
        if (instance.currentProfile == newProfile)
            return;

        instance.currentProfile = newProfile;
        instance.OnAlertStateChanged(GameManager.CurrentAlertState);
    }

    private void OnAlertStateChanged(GameManager.AlertState newState)
    {
        if (currentProfile == null)            
            return;

        AudioClip newTrack;
        switch (newState)
        {
            default:
            case GameManager.AlertState.Normal:
                newTrack = currentProfile.normal;
                break;
            case GameManager.AlertState.Caution:
                newTrack = currentProfile.caution;
                break;
            case GameManager.AlertState.Alert:
                newTrack = currentProfile.alert;
                break;
        }

        bool foundTrack = false;
        foreach (var musicSource in musicSources)
        {
            musicSource.isNeeded = musicSource.source.clip == newTrack;

            if (musicSource.source.clip == newTrack)
                foundTrack = true;
        }

        if (!foundTrack)
        {
            var source = Instantiate(musicSourcePrefab, transform);
            source.gameObject.name = newTrack.name;
            source.clip = newTrack;
            source.volume = 0;
            source.time = Time.timeSinceLevelLoad % newTrack.length;
            source.Play();

            musicSources.Add(new MusicSource()
            {
                source = source,
                isNeeded = true
            }); ;
        }
    }

    private void UpdateMusic()
    {
        if (musicSources.Count == 0)
            return;

        var maxVolume = MusicVolumeFromSettings(1);
        foreach (var musicSource in musicSources)
        {
            if (musicSource.source == null)
                continue;
            if (musicSource.isNeeded && musicSource.source.volume >= maxVolume)
                continue;

            if (!musicSource.isNeeded && musicSource.source.volume <= 0)
            {
                Destroy(musicSource.source.gameObject);
                continue;
            }

            var targetVolume = musicSource.isNeeded ? maxVolume : 0;
            var volume = Mathf.MoveTowards(musicSource.source.volume, targetVolume, Time.deltaTime * crossfadeSpeed);

            musicSource.source.volume = volume;
        }

        // Clean up dead sources, one at a time since lists dont like to be modified in loops
        foreach (var musicSource in musicSources)
        {
            if (musicSource.source != null)
                continue;

            musicSources.Remove(musicSource);
            break;
        }
    }

    float MusicVolumeFromSettings(float volume)
    {
        var settings = SettingsManager.Load();
        return volume * settings.MasterVolume * settings.MusicVolume;
    }
}
