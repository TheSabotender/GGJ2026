using UnityEngine;

public class CivilianBehavior : MonoBehaviour, IBehavior
{
    [SerializeField]
    private bool isPanicking = false;

    [SerializeField]
    private Vector3[] routine;

    private int currentRoutineIndex = 0;

    public void OnSeeAlien(AIBrain brain)
    {
        brain.lastKnownPlayerPos = GameSceneManager.PlayerBrain.transform.position;
        isPanicking = true;
        brain.StopWalking();
    }

    public void OnSeePanic(AIBrain brain, AIBrain triggeringEntity)
    {
        var region = RegionManager.GetRegionAtPosition(brain.transform.position);
        if (region.AlertState == AlertState.Caution)
        {
            isPanicking = true;
            brain.lastKnownPlayerPos = triggeringEntity.lastKnownPlayerPos;
            brain.StopWalking();
        }
    }

    public void SwitchState(AIBrain brain, AlertState newState)
    {
        if (newState == AlertState.Alert)
        {
            isPanicking = true;
            brain.StopWalking();
        }
    }

    public void TickAlert(AIBrain brain)
    {
        Panic();
    }

    public void TickCaution(AIBrain brain)
    {
        if (isPanicking)
            Panic();
    }

    public void TickIdle(AIBrain brain)
    {
        if (isPanicking)
            Panic();
        else 
            FollowRoutine(brain);
    }

    void FollowRoutine(AIBrain brain)
    {
        if (routine == null || routine.Length == 0)
            return;
        if (brain.IsWalking())
            return;

        Vector3 targetPos = routine[currentRoutineIndex];
        brain.GoToLocation(targetPos, isUrgent: false, onComplete: () =>
        {
            currentRoutineIndex = (currentRoutineIndex + 1) % routine.Length;
        });
    }

    void Panic()
    {
        // Implement panic behavior here
    }

    void OnDrawGizmosSelected()
    {
        if (routine == null || routine.Length == 0)
            return;

        bool first = true;
        Gizmos.color = Color.magenta;
        for (int i = 0; i < routine.Length; i++)
        {
            Vector3 point = routine[i];
            if (!first)
                Gizmos.DrawLine(routine[i-1], point);
            first = false;
            Gizmos.DrawWireSphere(point, 0.5f);
        }
    }
}
