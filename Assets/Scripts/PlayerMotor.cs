using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotor : MonoBehaviour
{
    [SerializeField]
    private EntityMotor currentMotor = null;

    [SerializeField]
    private InputActionReference moveAction = null;

    [SerializeField]
    private InputActionReference jumpAction = null;

    [SerializeField]
    private InputActionReference crouchAction = null;

    public EntityMotor CurrentMotor => currentMotor;

    private void OnEnable()
    {
        EnableActions();
    }

    private void OnDisable()
    {
        DisableActions();
    }

    private void Update()
    {
        if (currentMotor == null)
        {
            return;
        }

        if (moveAction != null && moveAction.action != null)
        {
            float moveInput = moveAction.action.ReadValue<float>();
            currentMotor.MoveHorizontal(moveInput);
        }

        if (jumpAction != null && jumpAction.action != null && jumpAction.action.WasPerformedThisFrame())
        {
            currentMotor.Jump();
        }

        if (crouchAction != null && crouchAction.action != null)
        {
            float crouchInput = crouchAction.action.ReadValue<float>();
            currentMotor.Crouch(crouchInput > 0.5f);
        }
    }

    public void SwapToHuman(HumanMotor humanMotor)
    {
        currentMotor = humanMotor;
    }

    public void SwapToAlien(AlienMotor alienMotor)
    {
        currentMotor = alienMotor;
    }

    private void EnableActions()
    {
        moveAction?.action?.Enable();
        jumpAction?.action?.Enable();
        crouchAction?.action?.Enable();
    }

    private void DisableActions()
    {
        moveAction?.action?.Disable();
        jumpAction?.action?.Disable();
        crouchAction?.action?.Disable();
    }
}
