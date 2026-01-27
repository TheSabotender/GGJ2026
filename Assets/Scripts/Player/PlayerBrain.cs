using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBrain : EntityBrain
{
    [SerializeField]
    private InputActionReference moveAction = null;

    [SerializeField]
    private InputActionReference jumpAction = null;

    [SerializeField]
    private InputActionReference crouchAction = null;

    [SerializeField]
    private InputActionReference maskMenuAction = null;

    [SerializeField]
    private CharacterProfile alienProfile;

    [SerializeField]
    private TendrilManager tendrilManager;

    [Header("Ground Check")]
    [SerializeField]
    private float groundCheckDistance = 0.1f;

    [SerializeField]
    private LayerMask groundLayers = ~0;

    public CharacterProfile DefaultProfile => alienProfile;

    public TendrilManager TendrilManager => tendrilManager;

    private bool isJumpHeld = false;

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
        if (GameManager.CurrentGameSave == null)
            return;
        if (MenuManager.CurrentScreen != MenuManager.Screen.None)
            return;

        if (currentMotor == null)
        {
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        if (moveAction != null && moveAction.action != null)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            currentMotor.MoveHorizontal(this, moveInput.x);
            currentMotor.MoveDepth(this, moveInput.y);
        }

        if (jumpAction != null && jumpAction.action != null)
        {
            bool jumpPressed = jumpAction.action.IsPressed();
            if (jumpPressed && !isJumpHeld)
            {
                currentMotor.Jump(this, IsGrounded());
            }
            else if (!jumpPressed && isJumpHeld && currentMotor is AlienMotor)
            {
                tendrilManager?.ReleaseTendril();
            }

            isJumpHeld = jumpPressed;
        }

        if (crouchAction != null && crouchAction.action != null)
        {
            float crouchInput = crouchAction.action.ReadValue<float>();
            currentMotor.Crouch(this, crouchInput > 0.5f);
        }
    }

    public void SwapMask(CharacterProfile profile)
    {
        if (profile == null)
        {
            Debug.LogError("Cannot swap to null profile");
            return;
        }

        //TODO change appearance of player to selected mask and change back to normal when unselected
        GameManager.CurrentGameSave.CurrentMask = profile.Guid;
        SwapMotor(profile.motor);
    }

    public void SwapMotor(EntityMotor newMotor)
    {
        currentMotor = newMotor;

        /*
        bool isAlien = currentMotor is AlienMotor;

        if (cachedRigidbody != null)
            cachedRigidbody.isKinematic = !isAlien;

        if (cachedCollider != null)
            cachedCollider.enabled = isAlien;
        */
    }

    private void EnableActions()
    {
        moveAction?.action?.Enable();
        jumpAction?.action?.Enable();
        crouchAction?.action?.Enable();
        if (maskMenuAction?.action != null)
        {
            maskMenuAction.action.performed -= OnMaskMenuAction;
            maskMenuAction.action.performed += OnMaskMenuAction;
            maskMenuAction.action.Enable();
        }
    }

    private void DisableActions()
    {
        moveAction?.action?.Disable();
        jumpAction?.action?.Disable();
        crouchAction?.action?.Disable();
        if (maskMenuAction?.action != null)
        {
            maskMenuAction.action.performed -= OnMaskMenuAction;
            maskMenuAction.action.Disable();
        }
    }

    private void OnMaskMenuAction(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        MenuManager.SetScreen(MenuManager.Screen.Mask);
    }

    private bool IsGrounded()
    {
        if (cachedCollider == null)
            return false;

        Bounds bounds = cachedCollider.bounds;
        float castDistance = bounds.extents.y + groundCheckDistance;
        Vector3 origin = bounds.center;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, castDistance, groundLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.collider != cachedCollider;
        }

        return false;
    }
}
