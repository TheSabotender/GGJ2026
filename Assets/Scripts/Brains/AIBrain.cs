using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class AIBrain : EntityBrain
{
    public CharacterProfile profile;
    public Animator animator;
    
    public Vector3 lastKnownPlayerPos;
    public bool canSeeAlien;
    public bool canSeePanic;
    public bool isDying;

    [Header("Plug in per-type behavior")]
    public MonoBehaviour behaviorComponent; // assign CivilianAlert, ScientistAlert, GuardAlert
    IBehavior alertBehavior;

    public bool IsAlive => !isDying;

    public override Animator Animator => animator;

    private GameManager.AlertState lastAlertState;
    private Coroutine movementCoroutine;

    protected override void Awake()
    {
        base.Awake();
        alertBehavior = (IBehavior)behaviorComponent;
    }

    protected override void Update()
    {
        if (isDying)
            return;
        if (GameManager.CurrentGameSave == null)
            return;
        if (MenuManager.CurrentScreen != MenuManager.Screen.None)
            return;
        if (currentMotor == null)
            return;
        HandleMovement();

        var seeAlien = LookForAlien();
        if (seeAlien != canSeeAlien)
        {
            canSeeAlien = seeAlien;
            if (canSeeAlien) alertBehavior.OnSeeAlien(this);
        }

        var seePanic = LookForPanic(out AIBrain triggeringEntity);
        if (seePanic != canSeePanic)
        {
            canSeePanic = seePanic;
            if (canSeePanic) alertBehavior.OnSeePanic(this, triggeringEntity);
        }

        // Example: perception updates lastKnownPlayerPos when player seen
        // if (CanSeePlayer()) lastKnownPlayerPos = player.position;
        if (lastAlertState != GameManager.CurrentAlertState)
        {
            lastAlertState = GameManager.CurrentAlertState;
            alertBehavior.SwitchState(this, lastAlertState);
        }

        if (lastAlertState == GameManager.AlertState.Normal) alertBehavior.TickIdle(this);
        else if (lastAlertState == GameManager.AlertState.Caution) alertBehavior.TickCaution(this);
        else if (lastAlertState == GameManager.AlertState.Alert) alertBehavior.TickAlert(this);
    }

    public bool IsWalking()
    {
        return movementCoroutine != null;
    }

    public void GoToLocation(Vector3 destination, bool isUrgent, Action onComplete)
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToRoutine(destination, isUrgent, onComplete));
    }

    private IEnumerator MoveToRoutine(Vector3 destination, bool isUrgent, Action onComplete)
    {
        var path = new NavigationPath(transform.position, destination, 1, currentMotor.LayerMask);

        // Follow the path points
        var lastPoint = -1;
        var nextPoint = 0;

        while (true)
        {
            // Check if we need to move towards the target point
            var targetPoint = path.Points[nextPoint];
            var distance = Vector3.Distance(transform.position, targetPoint);

            // Find the direction to move
            var isHorizontal = Mathf.Abs(targetPoint.x - transform.position.x) > 0.1f;
            var isDepth = Mathf.Abs(targetPoint.z - transform.position.z) > 0.1f;

            // Check if we reached the target point
            if (distance < 0.1f || (!isHorizontal && !isDepth))
            {
                // Reached the point
                lastPoint = nextPoint;
                nextPoint++;
                if (nextPoint >= path.Points.Count)
                    break;
                continue;
            }

            // Move towards the target point
            Vector3 direction = (targetPoint - transform.position).normalized;
            if (isDepth)
                currentMotor.MoveDepth(this, direction.z);
            else if (isHorizontal)
                currentMotor.MoveHorizontal(this, direction.x, true);

            yield return null;

        }

        currentMotor.MoveHorizontal(this, 0f, true);
        movementCoroutine = null;
        onComplete?.Invoke();
    }

    public void StopWalking()
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        movementCoroutine = null;
        currentMotor.MoveHorizontal(this, 0f, true);
    }

    public void Kill()
    {
        if (isDying)
            return;

        EnsurePhysicsComponents();

        isDying = true;
        GameManager.PlayerBrain.ObservationManager.RemoveObserver(this);

        // Disable motor, etc.
        if (Collider != null)
            Collider.isTrigger = true;
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        currentMotor.MoveHorizontal(this, 0f, true);
        currentMotor = null;

        // Additional death logic here
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // Visuals
        PlayAnimation(ANIMATOR_DEATH);

        // Wait for animation to finish
        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }

    bool LookForAlien()
    {
        if (CanSeePlayer())
        {
            var playerMask = GameManager.CurrentGameSave.Masks[GameManager.CurrentGameSave.CurrentMask];
            return (playerMask.status == MaskStatus.Compromised);
        }
        return false;
    }

    bool LookForPanic(out AIBrain triggeringEntity)
    {
        // Check if we can see any panicking NPCs nearby
        triggeringEntity = null;
        return false;
    }

    bool CanSeePlayer()
    {
        if (GameManager.PlayerBrain == null)
            return false;
        return GameManager.PlayerBrain.ObservationManager.CheckIfBeingObserved(this);
    }
}
