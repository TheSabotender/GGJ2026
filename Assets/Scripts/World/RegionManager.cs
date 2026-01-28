using System.Collections.Generic;
using UnityEngine;

public class RegionManager : MonoBehaviour
{
    [SerializeField]
    private float positionUpdateThreshold = 0.5f;

    private readonly List<WorldRegion> cachedRegions = new();
    private PlayerBrain playerBrain;
    private Vector3 lastPlayerPosition;

    public static WorldRegion CurrentRegion { get; private set; }

    private void Awake()
    {
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
        cachedRegions.AddRange(FindObjectsOfType<WorldRegion>(true));
    }

    private void UpdateCurrentRegion()
    {
        if (playerBrain == null)
        {
            return;
        }

        if (cachedRegions.Count == 0)
        {
            CurrentRegion = null;
            return;
        }

        var playerPosition = playerBrain.transform.position;
        var closestRegion = (WorldRegion)null;
        var closestDistance = float.PositiveInfinity;

        foreach (var region in cachedRegions)
        {
            if (region == null)
            {
                continue;
            }

            var distance = Vector3.Distance(playerPosition, region.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRegion = region;
            }
        }

        CurrentRegion = closestRegion;
    }
}
