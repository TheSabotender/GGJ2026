using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public partial class TendrilManager : MonoBehaviour
{
    [System.Serializable]
    private class FloorTendril
    {
        public Rope rope;
        public Coroutine coroutine;
        public bool isFree;
    }

    [Header("Floor Tendrils")]
    public bool UseFloorTendrils;
    [SerializeField] private int stickyCount = 5;
    [SerializeField] private float stickySpeed = 1f;
    [SerializeField] private float stickySpacing = 1f;

    private Vector3 lastPlayerPosition;
    private List<FloorTendril> floorTendrils;

    private void UpdateFloorTendrils()
    {
        var currentPosition = playerBrain.transform.position;
        if (Vector3.Distance(currentPosition, lastPlayerPosition) < stickySpacing)
            return;
        lastPlayerPosition = currentPosition;

        BreakOverlongTendrils();

        if (!UseFloorTendrils)
            return;

        // Attach new tendril
        if (GetTendrilHit(tendrilParent.position + Vector3.down, stickySpacing, out Vector3 tendrilDown))
            StickNewFloorTendril(tendrilDown);

        if (GetTendrilHit(tendrilParent.position + Vector3.up, stickySpacing, out Vector3 tendrilUp))
            StickNewFloorTendril(tendrilUp);

        if (GetTendrilHit(tendrilParent.position + Vector3.left, stickySpacing, out Vector3 tendrilLeft))
            StickNewFloorTendril(tendrilLeft);

        if (GetTendrilHit(tendrilParent.position + Vector3.right, stickySpacing, out Vector3 tendrilRight))
            StickNewFloorTendril(tendrilRight);
    }

    private void BreakOverlongTendrils()
    {
        if (floorTendrils == null || floorTendrils.Count == 0)
            return;

        foreach (var floorTendril in floorTendrils)
        {
            if (UseFloorTendrils && Vector3.Distance(tendrilParent.position, floorTendril.rope.EndPoint) < stickySpacing * 2)
                continue;

            if (floorTendril.coroutine != null)
                StopCoroutine(floorTendril.coroutine);
            floorTendril.coroutine = StartCoroutine(Unstick(floorTendril, stickySpeed));
            floorTendril.isFree = true;
        }
    }

    private FloorTendril GetFreeTendril()
    {
        var tendril = floorTendrils.FirstOrDefault(t => t.isFree);

        if (tendril != null)
            return tendril;

        // If there is no free tendril, but we have free slots
        if (floorTendrils.Count < stickyCount)
        {
            tendril = new FloorTendril() { rope = Instantiate(ropePrefab, tendrilParent) };
            floorTendrils.Add(tendril);
            return tendril;
        }

        // If tendril is still null, get the longest one
        var playerPos = GameManager.PlayerBrain.transform.position;
        var longestDist = 0f;
        foreach (var floorTendril in floorTendrils)
        {
            var thisDist = Vector3.Distance(playerPos, floorTendril.rope.EndPoint);
            if (thisDist < longestDist)
                continue;

            longestDist = thisDist;
            tendril = floorTendril;
        }

        return tendril;
    }

    private void StickNewFloorTendril(Vector3 target)
    {
        var tendril = GetFreeTendril();

        if (tendril == null)
            return;

        if (tendril.coroutine != null)
            StopCoroutine(tendril.coroutine);

        tendril.coroutine = StartCoroutine(Stick(tendril, stickySpeed, target));
        tendril.isFree = false;
    }

    private static IEnumerator Stick(FloorTendril tendril, float tendrilSpeed, Vector3 target)
    {
        tendril.rope.enabled = true;

        var playerBrain = GameManager.PlayerBrain;
        Vector3 lastTarget = tendril.isFree ? playerBrain.transform.position : tendril.rope.EndPoint;
        float distance = Vector3.Distance(lastTarget, target);
        float extendDuration = distance / tendrilSpeed;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            tendril.rope.StartPoint = playerBrain.transform.position;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            tendril.rope.EndPoint = Vector3.Lerp(lastTarget, target, t);

            yield return null;
        }

        tendril.rope.EndPoint = target;

        while (true)
        {
            tendril.rope.StartPoint = playerBrain.transform.position;
            yield return null;
        }
    }

    private static IEnumerator Unstick(FloorTendril tendril, float tendrilSpeed)
    {
        var playerBrain = GameManager.PlayerBrain;
        var target = tendril.rope.EndPoint;
        float distance = Vector3.Distance(playerBrain.transform.position, target);
        float retractDuration = distance / tendrilSpeed;

        float elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            tendril.rope.StartPoint = playerBrain.transform.position;
            tendril.rope.EndPoint = Vector3.Lerp(target, tendril.rope.StartPoint, t);
            yield return null;
        }

        tendril.rope.enabled = false;
        tendril.coroutine = null;
    }
}
