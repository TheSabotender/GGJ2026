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
        brain.transform.Translate(Vector3.up * jumpForce * Time.deltaTime, Space.World);
    }

    public override void Crouch(EntityBrain brain, bool isCrouching)
    {
        Vector3 targetScale = new Vector3(1f, isCrouching ? crouchScale : 1f, 1f);
        brain.transform.localScale = targetScale;
    }
}
