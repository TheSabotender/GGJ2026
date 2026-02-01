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
    private Vector3 lastPlayerPosition;

    private WorldRegion currentRegion;
    public static WorldRegion CurrentRegion => instance?.currentRegion;

    public static event Action<AlertState> AlertStateChanged;

    private void Awake()
    {
        instance = this;
        RefreshWorldRegions();
    }

    private void Start()
    {
        if (GameSceneManager.PlayerBrain != null)
        {
            lastPlayerPosition = GameSceneManager.PlayerBrain.transform.position;
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
        if (GameSceneManager.PlayerBrain == null || !GameSceneManager.IsGameLoaded)
            return;

        var currentPosition = GameSceneManager.PlayerBrain.transform.position;
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

    public static void UpdateCurrentRegion()
    {
        if (GameSceneManager.PlayerBrain == null)
            return;

        if (instance.cachedRegions.Count == 0)
        {
            instance.currentRegion = null;
            AudioManager.SetMusicProfile(null);
            return;
        }

        var playerPosition = GameSceneManager.PlayerBrain.transform.position;
        var newRegion = instance.currentRegion;
        foreach (var region in instance.cachedRegions)
        {
            if (region != null && region.IsWithinRegion(playerPosition))
            {
                newRegion = region;
                break;
            }
        }

        if (newRegion == instance.currentRegion)
            return;

        instance.currentRegion = newRegion;
        AudioManager.SetMusicProfile(CurrentRegion?.MusicProfile);
        AlertStateChanged?.Invoke(instance.currentRegion.AlertState);
    }

    public static WorldRegion GetRegionAtPosition(Vector3 position)
    {
        foreach (var region in instance.cachedRegions)
        {
            if (region != null && region.IsWithinRegion(position))
            {
                return region;
            }
        }
        return null;
    }
}
