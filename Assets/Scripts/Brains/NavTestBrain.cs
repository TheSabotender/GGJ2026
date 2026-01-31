using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavTestBrain : EntityBrain
{
    private Coroutine movementCoroutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            GoToLocation(new Vector3(0f, 0f, 0f), false);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            GoToLocation(new Vector3(3f, 0f, 0f), false);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            GoToLocation(new Vector3(6f, 0f, 0f), false);

        if (Input.GetKeyDown(KeyCode.Keypad4))
            GoToLocation(new Vector3(0f, 0f, -5f), false);
        if (Input.GetKeyDown(KeyCode.Keypad5))
            GoToLocation(new Vector3(3f, 0f, -5f), false);
        if (Input.GetKeyDown(KeyCode.Keypad6))
            GoToLocation(new Vector3(6f, 0f, -5f), false);
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

            // Find the direction to move
            var isHorizontal = Mathf.Abs(targetPoint.x - transform.position.x) > 0.1f;
            var isDepth = Mathf.Abs(targetPoint.z - transform.position.z) > 0.1f;

            // Check if we reached the target point
            if (distance < currentMotor.MoveSpeed || (!isHorizontal && !isDepth))
            {
                // Reached the point
                lastPoint = nextPoint;
                nextPoint++;
                if (nextPoint >= path.Points.Count)
                    break;
                continue;
            }

            // Move towards the target point
            Vector3 direction = (targetPoint - transform.position);
            if (isDepth)
                currentMotor.MoveDepth(this, direction.z);
            else if (isHorizontal)
                currentMotor.MoveHorizontal(this, direction.x, true);

            yield return null;

        }

        currentMotor.MoveHorizontal(this, 0f, true);
        movementCoroutine = null;
    }
}
