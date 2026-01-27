using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TendrilManager : MonoBehaviour
{
    [SerializeField] private InputActionReference mouseAction = null;
    [SerializeField] private LineRenderer lineRenderer = null;
    [SerializeField] private PlayerBrain playerBrain = null;
    [SerializeField] private float tendrilLength = 10f;
    [SerializeField] private float tendrilSpeed = 5f;
    [SerializeField] private float tendrilStrength = 10f;

    private Camera mainCam;
    private Coroutine coroutine;
    private InputDevice lastDevice;
    private bool isHoldingTendril;

    private void Awake()
    {
        mainCam = Camera.main;
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }

    public void LaunchTendril()
    {
        isHoldingTendril = true;
        if (coroutine != null)
            return;

        if (!TryGetAimWorldPoint(out Vector3 aimWorld))
            return;

        bool hitSomething = GetTendrilHit(aimWorld, out Vector3 tendrilEnd);

        if (hitSomething)
            coroutine = StartCoroutine(Latch(tendrilEnd));
        else
            coroutine = StartCoroutine(Miss(playerBrain.transform.position, tendrilEnd));
    }

    public void ReleaseTendril()
    {
        isHoldingTendril = false;
    }

    bool TryGetAimWorldPoint(out Vector3 aimWorld)
    {
        aimWorld = Vector3.zero;

        var action = mouseAction.action;
        if (action == null || !action.enabled || mainCam == null || playerBrain == null)
            return false;

        // Track the last used device (so we can decide mouse vs stick)
        var control = action.activeControl;
        if (control != null && control.device != lastDevice)
            lastDevice = control.device;

        // Decide which lane plane we're aiming on
        var brainPos = playerBrain.transform.position;
        float nearestLaneZ =
            (Mathf.Abs(brainPos.z - playerBrain.frontDepthZ) <= Mathf.Abs(brainPos.z - playerBrain.backDepthZ))
                ? playerBrain.frontDepthZ
                : playerBrain.backDepthZ;

        // Mouse path: screen -> ray -> plane intersection
            if (lastDevice is Mouse && Mouse.current != null)
            {            
                Vector2 mouseScreenPos = mouseAction.action.ReadValue<Vector2>();
                Ray ray = mainCam.ScreenPointToRay(mouseScreenPos);

                Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, nearestLaneZ));
                if (plane.Raycast(ray, out float enter))
                {
                    aimWorld = ray.GetPoint(enter);
                    aimWorld.z = nearestLaneZ; // keep it exact
                    return true;
                }

                return false;
            }

        // Stick path: input is already a direction; just build a world point on the lane
        Vector2 lookInput = action.ReadValue<Vector2>();
        if (lookInput.sqrMagnitude < 0.001f)
        {
            // No input: aim straight ahead (or at player position on lane)
            aimWorld = new Vector3(brainPos.x, brainPos.y, nearestLaneZ);
            return true;
        }

        float aimDistance = 5f; // tweak to taste
        Vector2 dir = lookInput.normalized;

        aimWorld = new Vector3(
            brainPos.x + dir.x * aimDistance,
            brainPos.y + dir.y * aimDistance,
            nearestLaneZ
        );
        return true;
    }

    bool GetTendrilHit(Vector3 targetWorld, out Vector3 hit)
    {
        Vector3 origin = playerBrain.transform.position;
        Vector3 dir = (targetWorld - origin);

        // Avoid NaNs if target == origin
        if (dir.sqrMagnitude < 0.0001f)
            dir = playerBrain.transform.forward;

        Ray ray = new Ray(origin, dir.normalized);

        if (Physics.Raycast(ray, out RaycastHit rayHit, tendrilLength))
        {
            hit = rayHit.point;
            return true;
        }

        hit = ray.GetPoint(tendrilLength);
        return false;
    }

    private IEnumerator Latch(Vector3 target)
    {
        var line = lineRenderer;
        if (line == null || playerBrain == null)
        {
            coroutine = null;
            yield break;
        }

        line.positionCount = 2;
        line.enabled = true;

        float speed = Mathf.Max(0.01f, tendrilSpeed);
        float distance = Vector3.Distance(playerBrain.transform.position, target);
        float extendDuration = distance / speed;
        float retractDuration = extendDuration;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            Vector3 start = playerBrain.transform.position;
            Vector3 end = Vector3.Lerp(start, target, t);
            line.SetPosition(0, start);
            line.SetPosition(1, end);

            Vector3 pullDirection = (target - start).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        while (isHoldingTendril)
        {
            Vector3 start = playerBrain.transform.position;
            Vector3 end = target;
            line.SetPosition(0, start);
            line.SetPosition(1, end);

            Vector3 pullDirection = (target - start).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            Vector3 start = playerBrain.transform.position;
            Vector3 end = Vector3.Lerp(target, start, t);
            line.SetPosition(0, start);
            line.SetPosition(1, end);

            Vector3 pullDirection = (target - start).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        line.enabled = false;

        coroutine = null;
        isHoldingTendril = false;
    }

    private IEnumerator Miss(Vector3 start, Vector3 end)
    {
        var line = lineRenderer;
        if (line == null || playerBrain == null)
        {
            coroutine = null;
            yield break;
        }

        line.positionCount = 2;
        line.enabled = true;

        float speed = Mathf.Max(0.01f, tendrilSpeed);
        float distance = Vector3.Distance(start, end);
        float extendDuration = distance / speed;
        float retractDuration = extendDuration;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            Vector3 currentStart = playerBrain.transform.position;
            Vector3 currentEnd = Vector3.Lerp(currentStart, end, t);
            line.SetPosition(0, currentStart);
            line.SetPosition(1, currentEnd);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            Vector3 currentStart = playerBrain.transform.position;
            Vector3 currentEnd = Vector3.Lerp(end, currentStart, t);
            line.SetPosition(0, currentStart);
            line.SetPosition(1, currentEnd);
            yield return null;
        }

        line.enabled = false;

        coroutine = null;
        isHoldingTendril = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || playerBrain == null || mainCam == null)
            return;

        if (!TryGetAimWorldPoint(out Vector3 aimWorld))
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(aimWorld, 0.5f);

        bool hit = GetTendrilHit(aimWorld, out Vector3 tendrilEnd);
        Gizmos.color = hit ? Color.green : Color.red;
        Gizmos.DrawLine(playerBrain.transform.position, tendrilEnd);
    }
}
