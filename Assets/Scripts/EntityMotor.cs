using System.Collections;
using UnityEngine;

public class EntityMotor : ScriptableObject
{
    [SerializeField] protected float moveSpeed = 5f;

    [SerializeField] protected LayerMask layerMask;

    // Units per second
    [SerializeField] private float depthTransitionSpeed = 5f;

    public virtual void MoveHorizontal(EntityBrain brain, float input)
    {
        if (Mathf.Approximately(input, 0f))
            return;

        if (brain.Rigidbody == null)
            return;

        Vector3 force = new Vector3(input * moveSpeed, 0f, 0f);
        brain.Rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    public virtual void MoveDepth(EntityBrain brain, float input)
    {
        if (Mathf.Approximately(input, 0f))
            return;

        // Ensure only one depth transition runs at a time
        if (brain.DepthTransitionRoutine != null)
            return;

        // Snap to nearest lane (so tiny drift doesnt compound)
        float z = brain.transform.position.z;
        float nearestLaneZ =
            (Mathf.Abs(z - brain.frontDepthZ) <= Mathf.Abs(z - brain.backDepthZ))
                ? brain.frontDepthZ
                : brain.backDepthZ;

        // Apply the snap for real
        var snapped = brain.transform.position;
        snapped.z = nearestLaneZ;
        brain.transform.position = snapped;

        // Decide target lane
        float targetZ = nearestLaneZ;
        if (input > 0f)
            targetZ = brain.backDepthZ;
        else if (input < 0f)
            targetZ = brain.frontDepthZ;

        if (Mathf.Approximately(nearestLaneZ, targetZ) || !CanChangeLane(brain))
            return;

        brain.DepthTransitionRoutine = brain.StartCoroutine(TransitionDepth(brain, nearestLaneZ, targetZ));
    }

    public virtual void Jump(EntityBrain brain, bool isGrounded) { }
    public virtual void Crouch(EntityBrain brain, bool isCrouching) { }

    private IEnumerator TransitionDepth(EntityBrain brain, float fromZ, float toZ)
    {
        float distance = Mathf.Abs(fromZ - toZ);

        // duration in seconds
        float duration = (depthTransitionSpeed <= 0f) ? 0f : (distance / depthTransitionSpeed);

        if (duration <= 0f)
        {
            var p = brain.transform.position;
            p.z = toZ;
            brain.transform.position = p;
            brain.DepthTransitionRoutine = null;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            float u = t / duration;

            var p = brain.transform.position;
            p.z = Mathf.Lerp(fromZ, toZ, u);
            brain.transform.position = p;

            t += Time.deltaTime;
            yield return null;
        }

        // Snap exactly to target at end
        var finalPos = brain.transform.position;
        finalPos.z = toZ;
        brain.transform.position = finalPos;

        brain.DepthTransitionRoutine = null;
    }

    public virtual bool CanChangeLane(EntityBrain brain)
    {
        float z = brain.transform.position.z;
        bool isInBackLane = Mathf.Abs(z - brain.frontDepthZ) > Mathf.Abs(z - brain.backDepthZ);

        var dir = isInBackLane ? Vector3.back : Vector3.forward;
        Ray ray = new Ray(brain.transform.position, dir);
        var distance = Mathf.Max(brain.frontDepthZ, brain.backDepthZ) - Mathf.Min(brain.frontDepthZ, brain.backDepthZ);
        if (Physics.Raycast(ray, distance, layerMask, QueryTriggerInteraction.Ignore))
            return false;

        return true;
    }
}
