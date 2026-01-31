using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public class AIBrain : EntityBrain
{
    public Animator animator;

    public Vector3 lastKnownPlayerPos;
    public bool canSeeAlien;
    public bool canSeePanic;

    [Header("Plug in per-type behavior")]
    public MonoBehaviour behaviorComponent; // assign CivilianAlert, ScientistAlert, GuardAlert
    IBehavior alertBehavior;

    public override Animator Animator => animator;

    private GameManager.AlertState lastAlertState;
    private Coroutine movementCoroutine;

    protected override void Awake()
    {
        alertBehavior = (IBehavior)behaviorComponent;
    }

    protected override void Update()
    {
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

        var seePanic = LookForPanic();
        if (seePanic != canSeePanic)
        {
            canSeePanic = seePanic;
            if (canSeePanic) alertBehavior.OnSeePanic(this);
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

    public void GoToLocation(Vector3 destination, bool isUrgent)
    {
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveToRoutine(destination, isUrgent));
    }

    private IEnumerator MoveToRoutine(Vector3 destination, bool isUrgent)
    {
        var currentLaneZ = EntityMotor.GetCurrentLane(this);
        var destinationLaneZ = EntityMotor.GetLaneFromPosition(destination);

        var path = new NavigationPath(transform.position, destination, 1, currentMotor.LayerMask);

        // Follow the path points
        var lastPoint = -1;
        var nextPoint = 0;

        while (true)
        {
            // Check if we need to move towards the target point
            var targetPoint = path.Points[nextPoint];
            var distance = Vector3.Distance(transform.position, targetPoint);
            if (distance > 0.1f)
            {
                // Move towards the target point
                var isHorizontal = Mathf.Abs(targetPoint.x - transform.position.x) > 0.1f;
                var isDepth = Mathf.Abs(targetPoint.z - transform.position.z) > 0.1f;

                Vector3 direction = (targetPoint - transform.position).normalized;
                if (isDepth)
                {
                    currentMotor.MoveDepth(this, direction.z);
                }
                else if (isHorizontal)
                    currentMotor.MoveHorizontal(this, direction.x, true);

                yield return null;
            }

            // Reached the target point
            else
            {
                lastPoint = nextPoint;
                nextPoint++;
                if (nextPoint >= path.Points.Count)
                    break;
            }
        }

        currentMotor.MoveHorizontal(this, 0f, true);
        movementCoroutine = null;
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

    bool LookForPanic()
    {
        // Check if we can see any panicking NPCs nearby
        return false;
    }

    bool CanSeePlayer()
    {
        // Implement line-of-sight logic here
        var player = GameManager.PlayerBrain.transform;
        return false;
    }
}
