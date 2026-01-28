using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    [SerializeField]
    private float positionUpdateThreshold = 0.5f;

    [SerializeField]
    private AudioSource musicSourceA;
    
    [SerializeField]
    private AudioSource musicSourceB;

    [SerializeField]
    private float crossfadeDuration;

    private readonly List<AudioSource> cachedAudioSources = new();
    private PlayerBrain playerBrain;
    private Vector3 lastPlayerPosition;
    private MusicProfile currentProfile;
    private AudioSource activeSource;

    private void Awake()
    {
        instance = this;
        RefreshAudioSources();
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
        switch (newState)
        {
            case GameManager.AlertState.Normal:
                TransitionMusic(currentProfile.normal);
                break;
            case GameManager.AlertState.Caution:
                TransitionMusic(currentProfile.caution);
                break;
            case GameManager.AlertState.Alert:
                TransitionMusic(currentProfile.alert);
                break;
        }
    }

    private IEnumerator TransitionMusic(AudioClip target)
    {
        var oldSource = activeSource;
        var newSource = activeSource == musicSourceA ? musicSourceB : musicSourceA;

        newSource.clip = target;
        newSource.volume = 0f;
        if (target != null)
            newSource.Play();

        float time = 0f;

        while (time < crossfadeDuration)
        {
            float t = time / crossfadeDuration;
            t = Mathf.SmoothStep(0f, 1f, t);

            oldSource.volume = MusicVolumeFromSettings(1f - t);
            if(target != null)
                newSource.volume = MusicVolumeFromSettings(t);

            time += Time.deltaTime;
            yield return null;
        }

        // Finalize volumes
        oldSource.volume = 0f;
        if (target != null)
            newSource.volume = MusicVolumeFromSettings(1f);

        oldSource.Stop();
        activeSource = newSource;
    }


    float MusicVolumeFromSettings(float volume)
    {
        var settings = SettingsManager.Load();
        return volume * settings.MasterVolume * settings.MusicVolume;
    }
}
