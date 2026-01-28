using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;

public class TendrilManager : MonoBehaviour
{
    [System.Serializable]
    private class FloorTendril
    {
        public Rope rope;
        public Coroutine coroutine;
        public bool isFree;
    }

    [SerializeField] private InputActionReference mouseAction = null;
    [SerializeField] private Rope ropePrefab;
    [SerializeField] private PlayerBrain playerBrain = null;
    [SerializeField] private LayerMask tendrilLayerMask;

    [Header("Floor Tendrils")]
    [SerializeField] private int stickyCount = 5;
    [SerializeField] private float stickySpeed = 1f;
    [SerializeField] private float stickySpacing = 1f;

    [Header("Launch Tendril")]
    [SerializeField] private float tendrilLength = 10f;
    [SerializeField] private Vector2 tendrilSpeed = new Vector2(20, 30);
    [SerializeField] private float tendrilStrength = 10f;
    [SerializeField] [Range(0,1)] private float tendrilElasticity = 0.8f;
    [SerializeField] private int tendrilCount = 5;
    [SerializeField] private float tendrilScatter = 1f;

    private Vector3 lastPlayerPosition;
    private List<FloorTendril> floorTendrils;

    private Camera mainCam;
    public static bool isHoldingTendril { get; private set; }

    private void Awake()
    {
        mainCam = Camera.main;
        floorTendrils = new();
    }

    private void LateUpdate()
    {
        var currentPosition = playerBrain.transform.position;
        if (Vector3.Distance(currentPosition, lastPlayerPosition) < stickySpacing)
            return;
        lastPlayerPosition = currentPosition;

        BreakOverlongTendrils();

        // Attach new tendril
        if (GetTendrilHit(playerBrain.transform.position + Vector3.down, stickySpacing, out Vector3 tendrilDown))
            StickNewFloorTendril(tendrilDown);

        if (GetTendrilHit(playerBrain.transform.position + Vector3.up, stickySpacing, out Vector3 tendrilUp))
            StickNewFloorTendril(tendrilUp);

        if (GetTendrilHit(playerBrain.transform.position + Vector3.left, stickySpacing, out Vector3 tendrilLeft))
            StickNewFloorTendril(tendrilLeft);

        if (GetTendrilHit(playerBrain.transform.position + Vector3.right, stickySpacing, out Vector3 tendrilRight))
            StickNewFloorTendril(tendrilRight);
    }

    private void BreakOverlongTendrils()
    {
        if (floorTendrils == null || floorTendrils.Count == 0)
            return;

        var playerPos = GameManager.PlayerBrain.transform.position;
        foreach (var floorTendril in floorTendrils)
        {
            if (Vector3.Distance(playerPos, floorTendril.rope.EndPoint) < stickySpacing * 2)
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
            tendril = new FloorTendril() { rope = Instantiate(ropePrefab, transform) };
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

    public void LaunchTendril()
    {
        isHoldingTendril = true;

        if (!Util.TryGetAimWorldPoint(mouseAction, out Vector3 aimWorld))
            return;

        for (int i = 0; i < tendrilCount; i++)
        {
            var random = new Vector3(Random.Range(-tendrilScatter, tendrilScatter), Random.Range(-tendrilScatter, tendrilScatter), 0);
            var hit = GetTendrilHit(aimWorld + random, tendrilLength, out Vector3 tendrilEnd);
            var rope = Instantiate(ropePrefab, transform);

            Debug.DrawLine(GameManager.PlayerBrain.transform.position, tendrilEnd, Color.red, 1);

            if (hit)
                rope.StartCoroutine(Latch(rope, tendrilEnd, tendrilSpeed, tendrilStrength / tendrilCount, tendrilElasticity));
            else
                rope.StartCoroutine(Miss(rope, tendrilEnd, tendrilSpeed, tendrilElasticity));
        }
    }

    public void ReleaseTendril()
    {
        isHoldingTendril = false;
    }

    bool GetTendrilHit(Vector3 targetWorld, float maxLength, out Vector3 hit)
    {
        Vector3 origin = playerBrain.transform.position;
        Vector3 dir = (targetWorld - origin);

        // Avoid NaNs if target == origin
        if (dir.sqrMagnitude < 0.0001f)
            dir = playerBrain.transform.forward;

        Ray ray = new Ray(origin, dir.normalized);

        if (Physics.Raycast(ray, out RaycastHit rayHit, maxLength, tendrilLayerMask, QueryTriggerInteraction.Ignore))
        {
            hit = rayHit.point;
            return true;
        }

        hit = ray.GetPoint(maxLength);
        return false;
    }

    private static IEnumerator Latch(Rope rope, Vector3 target, Vector2 tendrilSpeed, float tendrilStrength, float tendrilElasticity)
    {
        var playerBrain = GameManager.PlayerBrain;

        float distance = Vector3.Distance(playerBrain.transform.position, target);
        float extendDuration = distance / tendrilSpeed.x;
        float retractDuration = distance / tendrilSpeed.y;
        rope.Elasticity = tendrilElasticity;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = Vector3.Lerp(rope.StartPoint, target, t);
            rope.Elasticity = Mathf.Lerp(tendrilElasticity, 1, t);

            if(tendrilStrength > 0)
            {
                Vector3 pullDirection = (target - rope.StartPoint).normalized;
                //Vector3 pullDirection = (rope.NextPoint - rope.StartPoint).normalized;
                playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            }
            yield return null;
        }

        while (TendrilManager.isHoldingTendril)
        {
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = target;

            if (tendrilStrength > 0)
            {
                Vector3 pullDirection = (target - rope.StartPoint).normalized;
                //Vector3 pullDirection = (rope.NextPoint - rope.StartPoint).normalized;
                playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            }
            yield return null;
        }

        rope.Elasticity = tendrilElasticity;

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = Vector3.Lerp(target, rope.StartPoint, t);
            yield return null;
        }

        Destroy(rope.gameObject);
    }

    private static IEnumerator Miss(Rope rope, Vector3 target, Vector2 tendrilSpeed, float tendrilElasticity)
    {
        var playerTransform = GameManager.PlayerBrain.transform;
        rope.Elasticity = tendrilElasticity;

        float distance = Vector3.Distance(playerTransform.position, target);
        float extendDuration = distance / tendrilSpeed.x;
        float retractDuration = distance / tendrilSpeed.y;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            rope.StartPoint = playerTransform.position;
            rope.EndPoint = Vector3.Lerp(rope.StartPoint, target, t);

            yield return null;
        }

           elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            rope.StartPoint = playerTransform.position;
            rope.EndPoint = Vector3.Lerp(target, rope.StartPoint, t);
            yield return null;
        }

        Destroy(rope.gameObject);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || playerBrain == null || mainCam == null)
            return;

        if (!Util.TryGetAimWorldPoint(mouseAction, out Vector3 aimWorld))
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(aimWorld, 0.5f);

        bool hit = GetTendrilHit(aimWorld, tendrilLength, out Vector3 tendrilEnd);
        Gizmos.color = hit ? Color.green : Color.red;
        Gizmos.DrawLine(playerBrain.transform.position, tendrilEnd);
    }
}
