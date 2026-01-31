using UnityEngine;

public class CivilianBehavior : MonoBehaviour, IBehavior
{
    [SerializeField]
    private bool isPanicking = false;

    public void OnSeeAlien(AIBrain brain)
    {
        brain.lastKnownPlayerPos = GameManager.PlayerBrain.transform.position;
        isPanicking = true;
    }

    public void OnSeePanic(AIBrain brain, AIBrain triggeringEntity)
    {
        var region = RegionManager.GetRegionAtPosition(brain.transform.position);
        if (region.AlertState == GameManager.AlertState.Caution)
        {
            isPanicking = true;
            brain.lastKnownPlayerPos = triggeringEntity.lastKnownPlayerPos;
        }
    }

    public void SwitchState(AIBrain brain, GameManager.AlertState newState)
    {
        if (newState == GameManager.AlertState.Alert)
        {
            isPanicking = true;
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
    }

    void Panic()
    {
        // Implement panic behavior here
    }
}
