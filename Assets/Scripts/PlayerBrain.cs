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

    public CharacterProfile DefaultProfile => alienProfile;

    public TendrilManager TendrilManager => tendrilManager;


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

        if (jumpAction != null && jumpAction.action != null && jumpAction.action.WasPerformedThisFrame())
        {
            currentMotor.Jump(this);
        }

        if (crouchAction != null && crouchAction.action != null)
        {
            float crouchInput = crouchAction.action.ReadValue<float>();
            currentMotor.Crouch(this, crouchInput > 0.5f);
        }
    }

    public void SwapMask(CharacterProfile profile)
    {
        //TODO change appearance of player to selected mask and change back to normal when unselected
        SwapMotor(profile.motor);
    }

    public void SwapMotor(EntityMotor newMotor)
    {
        if (currentMotor != null)
            Destroy(currentMotor);

        currentMotor = newMotor;

        bool isAlien = currentMotor is AlienMotor;

        if (cachedRigidbody != null)
            cachedRigidbody.isKinematic = !isAlien;

        if (cachedCollider != null)
            cachedCollider.enabled = isAlien;
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
