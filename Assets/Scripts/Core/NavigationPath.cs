using System.Collections.Generic;
using UnityEngine;

public class NavigationPath
{
    public List<Vector3> Points { get; private set; }

    public NavigationPath(Vector3 start, Vector3 end, float heightAboveGround, LayerMask layerMask)
    {
        Points = new List<Vector3>();

        var Up = Vector3.up * heightAboveGround;
        start = new Vector3(start.x, start.y, EntityMotor.GetLaneFromPosition(start)) + Up;
        end = new Vector3(end.x, end.y, EntityMotor.GetLaneFromPosition(end)) + Up;

        var collision = Physics.Linecast(start, end, layerMask);
        if (Mathf.Approximately(start.z, end.z) && !collision)
        {
            // Same lane, direct path, no collision
            Points.Add(start - Up);
            Points.Add(end - Up);
            return;
        }

        Points.Add(end - Up);

        var endInThisLane = new Vector3(end.x, end.y, start.z);
        var foundTransition = false;
        if (Physics.Linecast(endInThisLane, start, out RaycastHit hit, layerMask))
        {
            //Found a transition area, add transition points
            if (FindTransitionArea(endInThisLane, hit.point, layerMask, out Vector3 tStart, out Vector3 tEnd))
            {
                Points.Add(tStart - Up);
                Points.Add(tEnd - Up);
                foundTransition = true;
            }
        } else
        {
            if (FindTransitionArea(endInThisLane, start, layerMask, out Vector3 tStart, out Vector3 tEnd))
            {
                Points.Add(tStart - Up);
                Points.Add(tEnd - Up);
                foundTransition = true;
            }
        }

        if (!foundTransition)
        {
            //Check the other way
            var direction = end.x - start.x;
            var otherDirectionEnd = new Vector3(endInThisLane.x - direction, end.y, end.z);
            if (Physics.Linecast(endInThisLane, otherDirectionEnd, out RaycastHit hitOther, layerMask))
            {
                //Found a transition area, add transition points
                if (FindTransitionArea(endInThisLane, hitOther.point, layerMask, out Vector3 tStart, out Vector3 tEnd))
                {
                    Points.Add(tStart - Up);
                    Points.Add(tEnd - Up);
                    foundTransition = true;
                }
            }
            else
            {
                if (FindTransitionArea(endInThisLane, endInThisLane + (Vector3.left * direction * 10f), layerMask, out Vector3 tStart, out Vector3 tEnd))
                {
                    Points.Add(tStart - Up);
                    Points.Add(tEnd - Up);
                    foundTransition = true;
                }
            }
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
            Debug.DrawLine(Points[i - 1] + Up, Points[i] + Up, Color.Lerp(Color.green, Color.red, t), 3f);
            Debug.DrawLine(Points[i - 1] + Up, Points[i] + Up, Color.Lerp(Color.green, Color.red, t), 3f);
        }
    }

    bool FindTransitionArea(Vector3 start, Vector3 end, LayerMask layerMask, out Vector3 tStart, out Vector3 tEnd)
    {
        var endInThisLane = new Vector3(end.x, end.y, start.z);
        var distance = Vector3.Distance(start, endInThisLane);
        for (float i = 0; i < distance; i += 0.1f)
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
}
