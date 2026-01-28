using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TendrilManager : MonoBehaviour
{
    [SerializeField] private InputActionReference mouseAction = null;
    [SerializeField] private Rope rope = null;
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
        if (rope != null)
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
            coroutine = StartCoroutine(Miss(playerBrain.transform.position, tendrilEnd));
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
        if (rope == null || playerBrain == null)
        {
            coroutine = null;
            yield break;
        }

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
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = Vector3.Lerp(rope.StartPoint, target, t);

            Vector3 pullDirection = (target - rope.StartPoint).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        while (isHoldingTendril)
        {
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = target;

            Vector3 pullDirection = (target - rope.StartPoint).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = Vector3.Lerp(target, rope.StartPoint, t);

            Vector3 pullDirection = (target - rope.StartPoint).normalized;
            playerBrain.Rigidbody.AddForce(pullDirection * tendrilStrength, ForceMode.Acceleration);
            yield return null;
        }

        rope.enabled = false;

        coroutine = null;
        isHoldingTendril = false;
    }

    private IEnumerator Miss(Vector3 start, Vector3 end)
    {
        if (rope == null || playerBrain == null)
        {
            coroutine = null;
            yield break;
        }

        rope.enabled = true;

        float speed = Mathf.Max(0.01f, tendrilSpeed);
        float distance = Vector3.Distance(start, end);
        float extendDuration = distance / speed;
        float retractDuration = extendDuration;

        float elapsed = 0f;
        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / extendDuration);
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = Vector3.Lerp(rope.StartPoint, end, t);            

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / retractDuration);
            rope.StartPoint = playerBrain.transform.position;
            rope.EndPoint = Vector3.Lerp(end, rope.StartPoint, t);
            yield return null;
        }

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
