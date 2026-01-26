using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]

public class EntityMotor : MonoBehaviour
{
    [SerializeField]
    protected float moveSpeed = 5f;

    protected Rigidbody cachedRigidbody;
    protected Collider cachedCollider;

    public virtual void MoveHorizontal(float input)
    {
        if (Mathf.Approximately(input, 0f))
        {
            return;
        }

        Vector3 movement = new Vector3(input * moveSpeed * Time.deltaTime, 0f, 0f);
        transform.Translate(movement, Space.World);
    }

    public virtual void Jump()
    {
    }

    public virtual void Crouch(bool isCrouching)
    {
    }

    protected virtual void Awake()
    {
        EnsurePhysicsComponents();
    }

    public virtual void OnBecomeActive()
    {
    }

    private void EnsurePhysicsComponents()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();

        if (cachedRigidbody == null)
        {
            cachedRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        if (cachedCollider == null)
        {
            cachedCollider = gameObject.AddComponent<BoxCollider>();
        }
    }
}
