using UnityEngine;

public class EntityMotor : ScriptableObject
{
    public enum DepthAvailability
    {
        HasFront,
        HasBack,
        HasBoth
    }

    [SerializeField]
    protected float moveSpeed = 5f;

    [SerializeField]
    private float depthDistance = 1f;

    [SerializeField]
    private float depthTransitionSpeed = 5f;

    private bool depthInitialized = false;
    private float frontDepthZ = 0f;
    private float backDepthZ = 0f;

    public virtual void MoveHorizontal(EntityBrain brain, float input)
    {
        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        Vector3 movement = new Vector3(input * moveSpeed * Time.deltaTime, 0f, 0f);
        brain.transform.Translate(movement, Space.World);
    }

    public virtual void MoveDepth(EntityBrain brain, float input)
    {
        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        if (!depthInitialized)
        {
            InitializeDepthPositions(brain);
        }

        DepthAvailability availability = CheckDepthAvailability(brain);
        float targetDepth = frontDepthZ;

        if (availability == DepthAvailability.HasBack)
        {
            targetDepth = backDepthZ;
        }
        else if (availability == DepthAvailability.HasBoth)
        {
            targetDepth = input > 0f ? backDepthZ : frontDepthZ;
        }

        Vector3 position = brain.transform.position;
        position.z = Mathf.MoveTowards(position.z, targetDepth, depthTransitionSpeed * Time.deltaTime);
        brain.transform.position = position;
    }

    public virtual DepthAvailability CheckDepthAvailability(EntityBrain brain)
    {
        return DepthAvailability.HasBoth;
    }

    public virtual void Jump(EntityBrain brain)
    {
    }

    public virtual void Crouch(EntityBrain brain, bool isCrouching)
    {
    }

    private void InitializeDepthPositions(EntityBrain brain)
    {
        frontDepthZ = brain.transform.position.z;
        backDepthZ = frontDepthZ + depthDistance;
        depthInitialized = true;
    }
}
