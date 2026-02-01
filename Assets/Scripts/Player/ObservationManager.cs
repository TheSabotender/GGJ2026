using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ObservationManager : MonoBehaviour
{
    [SerializeField]
    private LayerMask obstructionLayers = ~0;

    [SerializeField]
    private float viewDistance = 5f;

    private List<EntityBrain> observers = new List<EntityBrain>();

    public int ObserverCount => observers.Count;

    public List<EntityBrain> GetObservers()
    {
        return new List<EntityBrain>(observers);
    }

    public bool CheckIfBeingObserved(EntityBrain source)
    {
        //Check both the bottom and top of the target
        var startA = transform.position;
        var startB = transform.position + Vector3.up * 0.5f;

        //first check if within view distance
        if (Vector3.Distance(transform.position, source.transform.position) > viewDistance)
        {
            if (observers.Contains(source))
                observers.Remove(source);
            return false;
        }

        // then check line of sight
        if (Physics.Linecast(startA, source.transform.position, obstructionLayers)
            || Physics.Linecast(startB, source.GetEyesTransform().position, obstructionLayers))
        {
            if (observers.Contains(source))
                observers.Remove(source);
            return false;
        }
        
        if (!observers.Contains(source))
            observers.Add(source);
        return true;
    }

    public void RemoveObserver(EntityBrain source)
    {
        if (observers.Contains(source))
            observers.Remove(source);
    }
}
