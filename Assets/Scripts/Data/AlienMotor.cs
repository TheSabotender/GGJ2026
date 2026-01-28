using UnityEngine;

[CreateAssetMenu(fileName = "AlienMotor", menuName = "Data/Motors/AlienMotor")]
public class AlienMotor : EntityMotor
{
    /// <summary>
    /// Overriden to implement tendril launch behavior.
    /// </summary>
    /// <param name="transform"></param>
    public override void Jump(EntityBrain brain, bool isGrounded)
    {
        var playerBrain = brain as PlayerBrain;
        if (playerBrain?.TendrilManager == null)
            return;

        playerBrain.TendrilManager.LaunchTendril();
    }
}
