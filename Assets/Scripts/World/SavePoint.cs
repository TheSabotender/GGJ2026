using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    [SerializeField]
    private string guid;

    [SerializeField]
    private float interactionRange = 2f;

    private static readonly List<SavePoint> ActiveSavePoints = new();

    public string Guid => guid;

    private void OnEnable()
    {
        if (!ActiveSavePoints.Contains(this))
            ActiveSavePoints.Add(this);
    }

    private void OnDisable()
    {
        ActiveSavePoints.Remove(this);
    }

    public static SavePoint GetSavePointInRange(Vector3 position)
    {
        SavePoint closest = null;
        float closestSqrDistance = float.MaxValue;

        foreach (var savePoint in ActiveSavePoints)
        {
            if (savePoint == null)
                continue;

            var delta = savePoint.transform.position - position;
            var sqrDistance = delta.sqrMagnitude;
            var maxRange = savePoint.interactionRange;
            if (sqrDistance > maxRange * maxRange)
                continue;

            if (sqrDistance < closestSqrDistance)
            {
                closest = savePoint;
                closestSqrDistance = sqrDistance;
            }
        }

        return closest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
