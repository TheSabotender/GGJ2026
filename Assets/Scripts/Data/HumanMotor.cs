using UnityEngine;

[CreateAssetMenu(fileName = "HumanMotor", menuName = "Entity/Motors/HumanMotor")]
public class HumanMotor : EntityMotor
{
    [SerializeField]
    private float jumpForce = 5f;

    [SerializeField]
    private float crouchScale = 0.5f;

    public override void Jump(EntityBrain brain)
    {
        brain.Rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public override void Crouch(EntityBrain brain, bool isCrouching)
    {
        Vector3 targetScale = new Vector3(1f, isCrouching ? crouchScale : 1f, 1f);
        brain.transform.localScale = targetScale;
    }
}
