using UnityEngine;

public interface IBehavior
{
    void TickIdle(AIBrain brain);
    void TickCaution(AIBrain brain);
    void TickAlert(AIBrain brain);
    void SwitchState(AIBrain brain, GameManager.AlertState newState);
    void OnSeeAlien(AIBrain brain);
    void OnSeePanic(AIBrain brain);
}
