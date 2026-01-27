using UnityEngine;

[CreateAssetMenu(fileName = "AlienMotor", menuName = "Entity/Motors/AlienMotor")]
public class AlienMotor : EntityMotor
{
    public override void MoveHorizontal(EntityBrain brain, float input)
    {
        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        if (brain.Rigidbody == null)
        {
            base.MoveHorizontal(brain, input);
            return;
        }

        Vector3 force = new Vector3(input * moveSpeed, 0f, 0f);
        brain.Rigidbody.AddForce(force, ForceMode.Acceleration);
    }

    /// <summary>
    /// Overriden to implement tendril launch behavior.
    /// </summary>
    /// <param name="transform"></param>
    public override void Jump(EntityBrain brain)
    {
        var playerBrain = brain as PlayerBrain;
        if (playerBrain?.TendrilManager == null)
            return;

        playerBrain.TendrilManager.LaunchTendril();
    }
}
