using NUnit;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TendrilManager : MonoBehaviour
{
    [SerializeField] private InputActionReference mouseAction = null;
    [SerializeField] private Rope[] ropes = null;
    [SerializeField] private PlayerBrain playerBrain = null;
    [SerializeField] private float tendrilLength = 10f;
    [SerializeField] private float tendrilSpeed = 5f;
    [SerializeField] private float tendrilStrength = 10f;

    private Camera mainCam;
    private Coroutine coroutine;
    private bool isHoldingTendril;

    private void Awake()
    {
        mainCam = Camera.main;
        if (ropes != null && ropes.Length > 0)
            foreach(var rope in ropes)
                rope.enabled = false;
    }

    public void LaunchTendril()
    {
        isHoldingTendril = true;
        if (coroutine != null)
            return;

        if (!Util.TryGetAimWorldPoint(mouseAction, out Vector3 aimWorld))
            return;

        bool hitSomething = GetTendrilHit(aimWorld, out Vector3 tendrilEnd);

        if (hitSomething)
            coroutine = StartCoroutine(Latch(tendrilEnd));
        else
            coroutine = StartCoroutine(Miss(tendrilEnd));
    }

    public void ReleaseTendril()
    {
        isHoldingTendril = false;
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
        if (playerBrain == null || ropes == null || ropes.Length == 0)
        {
            coroutine = null;
            yield break;
        }

        foreach (var rope in ropes)
            rope.enabled = true;

        float speed = Mathf.Max(0.01f, tendrilSpeed);
        float distance = Vector3.Distance(playerBrain.transform.position, target);
        float extendDuration = distance / speed;
        float retractDuration = extendDuration;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            var start = playerBrain.transform.position;
            var end = Vector3.Lerp(start, target, t);

            foreach (var rope in ropes)
            {
                rope.StartPoint = start;
                rope.EndPoint = end;
            }

            Vector3 pullDirection = (target - start).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        while (isHoldingTendril)
        {
            var start = playerBrain.transform.position;
            var end = target;

            foreach (var rope in ropes)
            {
                rope.StartPoint = start;
                rope.EndPoint = end;
            }

            Vector3 pullDirection = (target - start).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            var start = playerBrain.transform.position;
            var end = Vector3.Lerp(target, start, t);

            foreach (var rope in ropes)
            {
                rope.StartPoint = start;
                rope.EndPoint = end;
            }

            Vector3 pullDirection = (target - start).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        foreach (var rope in ropes)
            rope.enabled = false;

        coroutine = null;
        isHoldingTendril = false;
    }

    private IEnumerator Miss(Vector3 target)
    {
        if (playerBrain == null || ropes == null || ropes.Length == 0)
        {
            coroutine = null;
            yield break;
        }

        foreach (var rope in ropes)
            rope.enabled = true;

        float speed = Mathf.Max(0.01f, tendrilSpeed);
        float distance = Vector3.Distance(playerBrain.transform.position, target);
        float extendDuration = distance / speed;
        float retractDuration = extendDuration;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            var start = playerBrain.transform.position;
            var end = Vector3.Lerp(start, target, t);

            foreach (var rope in ropes)
            {
                rope.StartPoint = start;
                rope.EndPoint = end;
            }

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            var start = playerBrain.transform.position;
            var end = Vector3.Lerp(target, start, t);

            foreach (var rope in ropes)
            {
                rope.StartPoint = start;
                rope.EndPoint = end;
            }
            yield return null;
        }

        foreach (var rope in ropes)
            rope.enabled = false;

        coroutine = null;
        isHoldingTendril = false;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || playerBrain == null || mainCam == null)
            return;

        if (!Util.TryGetAimWorldPoint(mouseAction, out Vector3 aimWorld))
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(aimWorld, 0.5f);

        bool hit = GetTendrilHit(aimWorld, out Vector3 tendrilEnd);
        Gizmos.color = hit ? Color.green : Color.red;
        Gizmos.DrawLine(playerBrain.transform.position, tendrilEnd);
    }
}
