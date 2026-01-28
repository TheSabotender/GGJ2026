using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using static GameManager;

public class RegionManager : MonoBehaviour
{
    private static RegionManager instance;

    [SerializeField]
    private float positionUpdateThreshold = 0.5f;

    private readonly List<WorldRegion> cachedRegions = new();
    private PlayerBrain playerBrain;
    private Vector3 lastPlayerPosition;

    private WorldRegion currentRegion;
    public static WorldRegion CurrentRegion => instance.currentRegion;

    public static event Action<AlertState> AlertStateChanged;

    private void Awake()
    {
        instance = this;
        RefreshWorldRegions();
    }

    private void Start()
    {
        playerBrain = GameManager.PlayerBrain;
        if (playerBrain != null)
        {
            lastPlayerPosition = playerBrain.transform.position;
            UpdateCurrentRegion();
        }
    }

    public static void SetAlertState(AlertState newState)
    {
        if (instance.currentRegion != null)
        {
            instance.currentRegion.SetAlertState(newState);
            AlertStateChanged?.Invoke(newState);
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
            UpdateCurrentRegion();
            return;
        }

        var currentPosition = playerBrain.transform.position;
        if ((currentPosition - lastPlayerPosition).sqrMagnitude < positionUpdateThreshold * positionUpdateThreshold)
        {
            return;
        }

        lastPlayerPosition = currentPosition;
        UpdateCurrentRegion();
    }

    public void RefreshWorldRegions()
    {
        cachedRegions.Clear();
        cachedRegions.AddRange(FindObjectsByType<WorldRegion>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private void UpdateCurrentRegion()
    {
        if (playerBrain == null)
            return;

        if (cachedRegions.Count == 0)
        {
            currentRegion = null;
            AudioManager.SetMusicProfile(null);
            return;
        }

        var playerPosition = playerBrain.transform.position;
        var newRegion = currentRegion;
        foreach (var region in cachedRegions)
        {
            if (region != null && region.IsWithinRegion(playerPosition))
            {
                newRegion = region;
                break;
            }
        }

        if (newRegion == currentRegion)
            return;

        currentRegion = newRegion;
        AudioManager.SetMusicProfile(CurrentRegion?.MusicProfile);
        AlertStateChanged?.Invoke(currentRegion.AlertState);
    }
}
