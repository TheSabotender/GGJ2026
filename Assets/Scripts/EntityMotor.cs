using UnityEngine;

public class EntityMotor : ScriptableObject
{
    [SerializeField]
    protected float moveSpeed = 5f;

    public virtual void MoveHorizontal(EntityBrain brain, float input)
    {
        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        Vector3 movement = new Vector3(input * moveSpeed * Time.deltaTime, 0f, 0f);
        brain.transform.Translate(movement, Space.World);
    }

    public virtual void Jump(EntityBrain brain)
    {
    }

    public virtual void Crouch(EntityBrain brain, bool isCrouching)
    {
    }
}
