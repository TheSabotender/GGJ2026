using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraTarget : MonoBehaviour
{
    [SerializeField] private InputActionReference mouseAction = null;

    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float maxDistanceFromPlayer;

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

        position.z = 0;
        cameraTarget.position = position;
    }
}
