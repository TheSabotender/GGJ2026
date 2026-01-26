using UnityEngine;

public class EntityBrain : MonoBehaviour
{
    [SerializeField]
    protected EntityMotor currentMotor = null;

    protected Rigidbody cachedRigidbody;
    protected Collider cachedCollider;

    public EntityMotor CurrentMotor => currentMotor;
    public Rigidbody Rigidbody => cachedRigidbody;
    public Collider Collider => cachedCollider;

    private void Awake()
    {
        EnsurePhysicsComponents();
    }

    private void EnsurePhysicsComponents()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        cachedCollider = GetComponent<Collider>();

        if (cachedRigidbody == null)
            cachedRigidbody = gameObject.AddComponent<Rigidbody>();

        if (cachedCollider == null)
            cachedCollider = gameObject.AddComponent<BoxCollider>();
    }
}
