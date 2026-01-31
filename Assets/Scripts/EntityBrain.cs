using UnityEngine;

public class EntityBrain : MonoBehaviour
{
    public const string ANIMATOR_IDLE = "Idle";
    public const string ANIMATOR_WALK = "Walk";
    public const string ANIMATOR_GO_UP = "GoUp";
    public const string ANIMATOR_GO_DOWN = "GoDown";
    public const string ANIMATOR_CROUCH = "Crouch";
    public const string ANIMATOR_CROUCHWALK = "CrouchWalk";
    public const string ANIMATOR_JUMP = "Jump";
    public const string ANIMATOR_FALL = "Fall";
    public const string ANIMATOR_DEATH = "Death";

    [SerializeField]
    protected EntityMotor currentMotor = null;

    protected Rigidbody cachedRigidbody;
    protected Collider cachedCollider;

    public EntityMotor CurrentMotor => currentMotor;
    public Rigidbody Rigidbody => cachedRigidbody;
    public Collider Collider => cachedCollider;

    public virtual Animator Animator => null;

    public Coroutine DepthTransitionRoutine;

    protected virtual void Awake()
    {
        DepthTransitionRoutine = null;
        EnsurePhysicsComponents();
    }

    protected virtual void Update()
    {
        if (GameManager.CurrentGameSave == null)
            return;
        if (MenuManager.CurrentScreen != MenuManager.Screen.None)
            return;
        if (currentMotor == null)
            return;
        HandleMovement();
    }

    protected virtual void HandleMovement()
    {

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

    public virtual void PlayAnimation(string state)
    {
        if (Animator == null)
            return;
        Animator.Play(state, 0);
    }
}
