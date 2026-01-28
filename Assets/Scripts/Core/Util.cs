using UnityEngine;
using UnityEngine.InputSystem;

public static class Util
{
    private static Camera mainCam;
    private static PlayerBrain playerBrain;
    private static InputDevice lastDevice;

    private static void EnsureReferences()
    {
        if (mainCam == null)
            mainCam = Camera.main;
        if (playerBrain == null)
            playerBrain = GameManager.PlayerBrain;
    }

    public static bool TryGetAimWorldPoint(InputActionReference mouseAction, out Vector3 aimWorld)
    {
        EnsureReferences();

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

    public static Vector3[] SimulateRope(Vector3 start, Vector3 end, float tension, float gravity, int fidelity)
    {
        return SimulateRope(start, end, tension, Vector3.down * gravity, fidelity);
    }

    public static Vector3[] SimulateRope(Vector3 start, Vector3 end, float tension, Vector3 gravity, int fidelity)
    {
        float distance = Vector3.Distance(start, end);
        int points = Mathf.Max(2, Mathf.CeilToInt(distance * Mathf.Max(1, fidelity)) + 1);

        Vector3[] ropePoints = new Vector3[points];
        Vector3 ropeDirection = end - start;

        if (ropeDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            for (int i = 0; i < points; i++)
                ropePoints[i] = start;
            return ropePoints;
        }

        float gravityMagnitude = gravity.magnitude;
        Vector3 gravityDirection = gravityMagnitude > Mathf.Epsilon ? gravity / gravityMagnitude : Vector3.down;
        Vector3 ropeDirectionNormalized = ropeDirection.normalized;

        Vector3 sagDirection = gravityDirection - Vector3.Dot(gravityDirection, ropeDirectionNormalized) * ropeDirectionNormalized;
        if (sagDirection.sqrMagnitude <= Mathf.Epsilon)
            sagDirection = Vector3.down;
        else
            sagDirection.Normalize();

        float clampedTension = Mathf.Clamp01(tension);
        float normalizedGravity = gravityMagnitude / Physics.gravity.magnitude;
        float sagAmplitude = (1f - clampedTension) * distance * 0.25f * normalizedGravity;

        for (int i = 0; i < points; i++)
        {
            float t = points == 1 ? 0f : i / (float)(points - 1);
            float sag = 4f * t * (1f - t) * sagAmplitude;
            ropePoints[i] = start + ropeDirection * t + sagDirection * sag;
        }

        return ropePoints;
    }
}
