using UnityEngine;

public class HumanMotor : EntityMotor
{
    [SerializeField]
    private float jumpForce = 5f;

    [SerializeField]
    private float crouchScale = 0.5f;

    public override void Jump()
    {
        transform.Translate(Vector3.up * jumpForce * Time.deltaTime, Space.World);
    }

    public override void Crouch(bool isCrouching)
    {
        Vector3 targetScale = new Vector3(1f, isCrouching ? crouchScale : 1f, 1f);
        transform.localScale = targetScale;
    }

    public override void OnBecomeActive()
    {
        if (cachedRigidbody != null)
        {
            cachedRigidbody.isKinematic = true;
        }

        if (cachedCollider != null)
        {
            cachedCollider.enabled = false;
        }
    }
}
