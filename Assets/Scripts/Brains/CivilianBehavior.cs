using UnityEngine;

public class CivilianBehavior : MonoBehaviour, IBehavior
{
    public void OnSeeAlien(AIBrain brain)
    {
        throw new System.NotImplementedException();
    }

    public void OnSeePanic(AIBrain brain)
    {
        throw new System.NotImplementedException();
    }

    public void SwitchState(AIBrain brain, GameManager.AlertState newState)
    {
        throw new System.NotImplementedException();
    }

    public void TickAlert(AIBrain brain)
    {
        throw new System.NotImplementedException();
    }

    public void TickCaution(AIBrain brain)
    {
        throw new System.NotImplementedException();
    }

    public void TickIdle(AIBrain brain)
    {
        throw new System.NotImplementedException();
    }
}
