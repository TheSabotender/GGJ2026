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

    private MaskButton[] buttons;
    private InputDevice lastDevice;
    private Vector2 currentMousePos;

    private void OnEnable()
    {
        var maskIds = GameManager.CurrentGameSave?.Masks;

        if (maskIds == null || maskIds.Count == 0)
            return;

        PopulateMenu(maskIds);
    }

    private void Update()
    {
        if (buttons == null || buttons.Length <= 0)
            return;

        var lastPos = currentMousePos;
        UpdateMousePos();
        var delta = currentMousePos - lastPos;
        
        foreach(var button in buttons)
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

    void UpdateMousePos()
    {
        var action = mouseAction.action;
        if (action == null || !action.enabled)
            return;

        // Track the last used device (so we can decide mouse vs stick)
        var control = action.activeControl;
        if (control != null && control.device != lastDevice)
            lastDevice = control.device;

        // Mouse path: input is already screen position
        if (lastDevice is Mouse && Mouse.current != null)
        {
            currentMousePos = mouseAction.action.ReadValue<Vector2>();
        }
        // Stick path: input is a direction;
        else
        {
            Vector2 lookInput = action.ReadValue<Vector2>();
            currentMousePos += lookInput;
        }
    }

    public void PopulateMenu(List<string> maskIds)
    {
        if (buttons != null && buttons.Length > 0)
            foreach (var child in buttons)
                Destroy(child.gameObject);

        buttons = new MaskButton[maskIds.Count];
        for (var i = 0; i < maskIds.Count; i++)
        {
            var maskProfile = GameManager.AllProfiles.FirstOrDefault(m => m.Guid == maskIds[i]);
            if (maskProfile == null)
            {
                Debug.LogWarning($"MaskMenu: Could not find mask profile with ID {maskIds[i]}");
                continue;
            }

            MaskButton buttonObj = Instantiate(maskButton, buttonContainer);
            buttonObj.Setup(maskProfile);
            buttons[i] = buttonObj;
        }
    }
}
