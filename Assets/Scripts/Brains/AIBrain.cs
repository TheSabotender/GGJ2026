using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private bool waitingForGameplay;

    protected override void Awake()
    {
        alertBehavior = (IBehavior)behaviorComponent;
    }

    private void Start()
    {
        if (MenuManager.CurrentScreen == MenuManager.Screen.Main)
        {
            waitingForGameplay = true;
            MenuManager.ScreenChanged += HandleScreenChanged;
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (MenuManager.CurrentScreen == MenuManager.Screen.Main)
            return;
        DespawnIfPlayerHasProfile();
    }

    private void OnDestroy()
    {
        MenuManager.ScreenChanged -= HandleScreenChanged;
    }

    private void HandleScreenChanged(MenuManager.Screen screen)
    {
        if (!waitingForGameplay || screen != MenuManager.Screen.None)
            return;
        waitingForGameplay = false;
        gameObject.SetActive(true);
    }

    private void DespawnIfPlayerHasProfile()
    {
        var save = GameManager.CurrentGameSave;
        if (save == null || save.Masks == null || profile == null)
            return;

        foreach (var mask in save.Masks)
        {
            var maskProfile = GameManager.AllProfiles.FirstOrDefault(candidate => candidate.Guid == mask.guid);
            if (maskProfile != null && maskProfile.Guid == profile.Guid)
            {
                Destroy(gameObject);
                return;
            }
        }
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
