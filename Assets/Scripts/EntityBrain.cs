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

    public float frontDepthZ = -1f;
    public float backDepthZ = 0f;
    public Coroutine DepthTransitionRoutine;

    private void Awake()
    {
        DepthTransitionRoutine = null;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(transform.position.x - 1f, transform.position.y, frontDepthZ),
                        new Vector3(transform.position.x + 1f, transform.position.y, frontDepthZ));

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(new Vector3(transform.position.x - 1f, transform.position.y, backDepthZ),
                        new Vector3(transform.position.x + 1f, transform.position.y, backDepthZ));
    }
}
