using UnityEngine;

public class AlienMotor : EntityMotor
{
    public override void MoveHorizontal(float input)
    {
        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        if (cachedRigidbody == null)
        {
            base.MoveHorizontal(input);
            return;
        }

        Vector3 force = new Vector3(input * moveSpeed, 0f, 0f);
        cachedRigidbody.AddForce(force, ForceMode.Acceleration);
    }

    public void LaunchTendril()
    {
        // Stub for future tendril launch behavior.
    }

    public override void OnBecomeActive()
    {
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = false;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = true;
        }
    }
}
