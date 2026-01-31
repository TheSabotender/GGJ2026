using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraTarget : MonoBehaviour
{
    [SerializeField] private InputActionReference mouseAction = null;

    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float maxDistanceFromPlayer;

    [SerializeField] private float minimumHeight = 0f;

    [SerializeField] private float minimumHeightSlowRange = 1f;

    private void Update()
    {
        var position = transform.position;

        if (GameManager.CurrentGameSave == null || (MenuManager.CurrentScreen != MenuManager.Screen.None /*&& MenuManager.CurrentScreen != MenuManager.Screen.Mask*/))
        {
            //lerp back?
        }
        else if(Util.TryGetAimWorldPoint(mouseAction, out Vector3 aimWorld))
        {
            var newPos = Vector3.Lerp(position, aimWorld, 0.5f);
            position = Vector3.MoveTowards(position, newPos, maxDistanceFromPlayer);
        }

        position.y = ApplyMinimumHeight(position.y);
        position.z = 0;
        cameraTarget.position = position;
    }

    private float ApplyMinimumHeight(float height)
    {
        if (height <= minimumHeight)
        {
            return minimumHeight;
        }

        if (minimumHeightSlowRange <= 0f || height >= minimumHeight + minimumHeightSlowRange)
        {
            return height;
        }

        var t = Mathf.InverseLerp(minimumHeight, minimumHeight + minimumHeightSlowRange, height);
        var eased = Mathf.SmoothStep(0f, 1f, t);
        return Mathf.Lerp(minimumHeight, minimumHeight + minimumHeightSlowRange, eased);
    }
}
