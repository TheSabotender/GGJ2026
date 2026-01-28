using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LightTransport;
using UnityEngine.UI;

public class MaskMenu : SubMenu
{
    public InputActionReference mouseAction;
    public float cursorForce = 100f;

    public MaskButton maskButton;
    public Transform buttonContainer;

    public bool worldMode;
    public float worldOffset;
    public float worldSmoothness;

    private MaskButton[] buttons;
    private InputDevice lastDevice;
    private Vector2 currentMousePos;

    private void OnEnable()
    {
        var maskStates = GameManager.CurrentGameSave?.Masks;

        if (maskStates == null || maskStates.Count == 0)
            return;

        transform.localScale = worldMode ? Vector3.one * 0.7f : Vector3.one;
        if (worldMode)
            UpdateWindowPosition(Vector2.zero, true);

        PopulateMenu(maskStates);
    }

    private void Update()
    {
        var lastPos = currentMousePos;
        currentMousePos = UpdateMousePos();
        var delta = currentMousePos - lastPos;

        if (worldMode)
            UpdateWindowPosition(delta, false);

        if (buttons == null || buttons.Length <= 0)
            return;
        foreach (var button in buttons)
        {
            var rb = button?.Rigidbody;
            if (rb == null)
                continue;

            var distance = Vector2.Distance(currentMousePos, rb.position);
            if (distance > 300f)
                continue;

            var distanceFactor = 1f - (distance / 300f);
            rb.AddForce(delta * cursorForce * distanceFactor);
        }
    }

    private void UpdateWindowPosition(Vector3 delta, bool instant)
    {
        var playerPos = GameManager.PlayerBrain.transform.position + (Vector3.up * worldOffset);
        var screenSpacePlayer = Camera.main.WorldToScreenPoint(playerPos);

        if (instant)
            transform.position = screenSpacePlayer;
        else
            transform.position = Vector3.MoveTowards(transform.position, screenSpacePlayer, Time.deltaTime * worldSmoothness);
    }

    Vector2 UpdateMousePos()
    {
        var action = mouseAction.action;
        if (action == null || !action.enabled)
            return currentMousePos;

        // Track the last used device (so we can decide mouse vs stick)
        var control = action.activeControl;
        if (control != null && control.device != lastDevice)
            lastDevice = control.device;

        // Mouse path: input is already screen position
        if (lastDevice is Mouse && Mouse.current != null)
        {
            return mouseAction.action.ReadValue<Vector2>();
        }
        // Stick path: input is a direction;
        else
        {
            Vector2 lookInput = action.ReadValue<Vector2>();
            return currentMousePos + lookInput;
        }
    }

    public void PopulateMenu(List<MaskState> maskIds)
    {
        if (buttons != null && buttons.Length > 0)
            foreach (var child in buttons)
                Destroy(child.gameObject);

        buttons = new MaskButton[maskIds.Count];
        for (var i = 0; i < maskIds.Count; i++)
        {
            var maskProfile = GameManager.AllProfiles.FirstOrDefault(m => m.Guid == maskIds[i].guid);
            if (maskProfile == null)
            {
                Debug.LogWarning($"MaskMenu: Could not find mask profile with ID {maskIds[i].guid}");
                continue;
            }

            MaskButton buttonObj = Instantiate(maskButton, buttonContainer);
            buttonObj.Setup(maskIds[i], maskProfile);
            buttons[i] = buttonObj;
        }
    }
}
