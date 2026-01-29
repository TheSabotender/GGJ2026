using UnityEngine;

[CreateAssetMenu(fileName = "AlienMotor", menuName = "Data/Motors/AlienMotor")]
public class AlienMotor : EntityMotor
{
    [SerializeField]
    private float jumpForce = 5f;

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

        if (isGrounded)
            brain.Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public override void Crouch(EntityBrain brain, bool isCrouching)
    {
        var playerBrain = brain as PlayerBrain;
        if (playerBrain?.TendrilManager == null)
            return;

        if (isCrouching)
            playerBrain.TendrilManager.SpreadTendrils();
        else
            playerBrain.TendrilManager.ReleaseSpread();
    }
}
