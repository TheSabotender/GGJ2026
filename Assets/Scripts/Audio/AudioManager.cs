using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private float positionUpdateThreshold = 0.5f;

    private readonly List<AudioSource> cachedAudioSources = new();
    private PlayerBrain playerBrain;
    private Vector3 lastPlayerPosition;

    private void Awake()
    {
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
        cachedAudioSources.AddRange(FindObjectsOfType<AudioSource>(true));
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
}
