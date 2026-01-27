using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraTarget : MonoBehaviour
{
    [SerializeField] private InputActionReference mouseAction = null;

    [SerializeField] private Transform cameraTarget;

    [SerializeField] private float maxDistanceFromPlayer;

    private Camera mainCam;
    private InputDevice lastDevice;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void Update()
    {
        var position = transform.position;

        if (GameManager.CurrentGameSave == null || MenuManager.CurrentScreen != MenuManager.Screen.None)
        {
            //lerp back?
        }
        else if(TryGetAimWorldPoint(out Vector3 aimWorld))
        {
            var newPos = Vector3.Lerp(position, aimWorld, 0.5f);
            position = Vector3.MoveTowards(position, newPos, maxDistanceFromPlayer);
        }

        position.z = 0;
        cameraTarget.position = position;
    }

    bool TryGetAimWorldPoint(out Vector3 aimWorld)
    {
        aimWorld = Vector3.zero;

        var action = mouseAction.action;
        if (action == null || !action.enabled || mainCam == null)
            return false;

        // Track the last used device (so we can decide mouse vs stick)
        var control = action.activeControl;
        if (control != null && control.device != lastDevice)
            lastDevice = control.device;

        // Mouse path: screen -> ray -> plane intersection
        if (lastDevice is Mouse && Mouse.current != null)
        {
            Vector2 mouseScreenPos = mouseAction.action.ReadValue<Vector2>();
            Ray ray = mainCam.ScreenPointToRay(mouseScreenPos);

            Debug.DrawRay(ray.origin, ray.direction);
            Debug.Log($"Mouse pos: {mouseScreenPos}");

            Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, 0));
            if (plane.Raycast(ray, out float enter))
            {
                aimWorld = ray.GetPoint(enter);
                aimWorld.z = 0; // keep it exact
                return true;
            }

            return false;
        }

        // Stick path: input is already a direction; just build a world point on the lane
        Vector2 lookInput = action.ReadValue<Vector2>();
        if (lookInput.sqrMagnitude < 0.001f)
        {
            aimWorld = transform.position;
            return true;
        }

        float aimDistance = 5f; // tweak to taste
        Vector2 dir = lookInput.normalized;

        aimWorld = new Vector3(
            transform.position.x + dir.x * aimDistance,
            transform.position.y + dir.y * aimDistance,
            0
        );
        return true;
    }
}
