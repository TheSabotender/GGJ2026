using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VolumeSetter : MonoBehaviour
{
    private AudioSource cachedAudioSource;
    private bool hasCachedBaseVolume;
    private float baseVolume = 1f;

    private void OnEnable()
    {
        EnsureAudioSource();

        if (!hasCachedBaseVolume && cachedAudioSource != null)
        {
            baseVolume = cachedAudioSource.volume;
            hasCachedBaseVolume = true;
        }

        ApplyVolume();

        SettingsManager.MasterVolumeChanged += HandleVolumeChanged;
        SettingsManager.SoundVolumeChanged += HandleVolumeChanged;
    }

    private void OnDisable()
    {
        SettingsManager.MasterVolumeChanged -= HandleVolumeChanged;
        SettingsManager.SoundVolumeChanged -= HandleVolumeChanged;
    }

    private void HandleVolumeChanged(float value)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        ApplyVolume();
    }

    private void ApplyVolume()
    {
        if (cachedAudioSource == null)
        {
            return;
        }

        var settings = SettingsManager.Load();
        cachedAudioSource.volume = baseVolume * settings.SoundVolume * settings.MasterVolume;
    }

    private void EnsureAudioSource()
    {
        if (cachedAudioSource == null)
        {
            cachedAudioSource = GetComponent<AudioSource>();
        }
    }
}
