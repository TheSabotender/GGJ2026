using System.Collections.Generic;
using UnityEngine;

public class NavigationPath
{
    private const float TransitionCheckStep = 0.5f;

    private class TransitionCheck
    {
        public Vector3 Start;
        public Vector3 End;
        public float Distance;
    }

    public List<Vector3> Points { get; private set; }

    public NavigationPath(Vector3 start, Vector3 end, float heightAboveGround, LayerMask layerMask)
    {
        Points = new List<Vector3>();

        var Up = Vector3.up * heightAboveGround;
        start = new Vector3(start.x, start.y, EntityMotor.GetLaneFromPosition(start)) + Up;
        end   = new Vector3(end.x,   end.y,   EntityMotor.GetLaneFromPosition(end))   + Up;

        var collision = Physics.Linecast(start, end, layerMask);
        if (Mathf.Approximately(start.z, end.z) && !collision)
        {
            // Same lane, direct path, no collision
            Points.Add(start - Up);
            Points.Add(end - Up);

            Debug.DrawLine(start, end, Color.green, 3f);
            Debug.DrawLine(start - Up, end - Up, Color.green, 3f);
            return;
        }

        Points.Add(end - Up);

        var endInThisLane = new Vector3(end.x, end.y, start.z);
        var foundTransition = false;
        var tStart = Vector3.zero;
        var tEnd = Vector3.zero;

        //Check the left
        var endLeft = endInThisLane + (Vector3.left * 10f);
        TransitionCheck left = null;
        if (Physics.Linecast(endInThisLane, endLeft, out RaycastHit hitLeft, layerMask))
            endLeft = hitLeft.point;
        if (FindTransitionArea(endInThisLane, endLeft, layerMask, out tStart, out tEnd))
        {
            left = new TransitionCheck()
            {
                Start = tStart,
                End = tEnd,
                Distance = Vector3.Distance(endInThisLane, tStart)
            };
            foundTransition = true;
        }

        //Check the right
        var endRight = endInThisLane + (Vector3.right * 10f);
        TransitionCheck right = null;
        if (Physics.Linecast(endInThisLane, endRight, out RaycastHit hitRight, layerMask))
            endRight = hitRight.point;
        if (FindTransitionArea(endInThisLane, endRight, layerMask, out tStart, out tEnd))
        {
            right = new TransitionCheck()
            {
                Start = tStart,
                End = tEnd,
                Distance = Vector3.Distance(endInThisLane, tStart)
            };
            foundTransition = true;
        }

        //Choose the closest transition
        if (foundTransition)
        {
            var best = CompareOptions(start, end, left, right);
            Points.Add(best.End - Up);
            Points.Add(best.Start - Up);
        }

        Points.Add(start - Up);
        Points.Reverse();

        Debug.Log($"NavigationPath created with {Points.Count} points from {start - Up} to {end - Up}");
        bool first = true;
        for (int i = 0; i < Points.Count; i++)
        {
            if (first)
            {
                first = false;
                continue;
            }

            var t = (i - 1) / (Points.Count - 1f);
            Debug.DrawLine(Points[i - 1],      Points[i],      Color.Lerp(Color.green, Color.red, t), 3f);
            Debug.DrawLine(Points[i - 1] + Up, Points[i] + Up, Color.Lerp(Color.green, Color.red, t), 3f);
        }
    }

    bool FindTransitionArea(Vector3 start, Vector3 end, LayerMask layerMask, out Vector3 tStart, out Vector3 tEnd)
    {
        var endInThisLane = new Vector3(end.x, end.y, start.z);
        var distance = Vector3.Distance(start, endInThisLane);
        for (float i = 0; i < distance; i += TransitionCheckStep)
        {
            var t = i / distance;
            var checkPoint = Vector3.Lerp(start, endInThisLane, t);

            //Check if we can change lane here
            var oppositeLaneZ = Mathf.Approximately(checkPoint.z, GameManager.BackDepthZ) ? GameManager.FrontDepthZ : GameManager.BackDepthZ;
            var collisionAtLaneChange = Physics.Linecast(checkPoint, new Vector3(checkPoint.x, checkPoint.y, oppositeLaneZ), layerMask);
            if (!collisionAtLaneChange)
            {
                //Can change lane here
                tStart = checkPoint;
                tEnd = new Vector3(checkPoint.x, checkPoint.y, oppositeLaneZ);
                return true;
            }
        }

        tStart = Vector3.zero;
        tEnd = Vector3.zero;
        return false;
    }

    TransitionCheck CompareOptions(Vector3 pathStart, Vector3 pathEnd, TransitionCheck a, TransitionCheck b)
    {
        if (a == null) return b;
        if (b == null) return a;

        // If A is between start and end, prefer it
        if (a.Start.x >= Mathf.Min(pathStart.x, pathEnd.x) && a.Start.x <= Mathf.Max(pathStart.x, pathEnd.x))
            return a;

        // If B is between start and end, prefer it
        if (b.Start.x >= Mathf.Min(pathStart.x, pathEnd.x) && b.Start.x <= Mathf.Max(pathStart.x, pathEnd.x))
            return b;

        // Otherwise, choose the closest

        return (a.Distance < b.Distance) ? a : b;
    }
}
