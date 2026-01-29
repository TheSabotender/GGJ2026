using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBrain : EntityBrain
{
    private class MaskInstance
    {
        public CharacterProfile profile;
        public CharacterPrefab instance;
        public Animator animator;
    }

    [SerializeField]
    private InputActionReference pauseMenuAction = null;

    [SerializeField]
    private InputActionReference maskMenuAction = null;

    [SerializeField]
    private InputActionReference moveAction = null;

    [SerializeField]
    private InputActionReference jumpAction = null;

    [SerializeField]
    private InputActionReference crouchAction = null;

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
    private bool isCrouchHeld = false;

    private List<MaskInstance> loadedMasks;
    private MaskInstance currentMask;

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
            bool crouchPressed = crouchAction.action.IsPressed();
            if (crouchPressed && !isCrouchHeld)
            {
                currentMotor.Crouch(this, true);
            }
            else if (!crouchPressed && isCrouchHeld)
            {
                currentMotor.Crouch(this, false);
            }
            isCrouchHeld = crouchPressed;
        }
    }

    public void PlayAnimation(string triggerName)
    {
        if (currentMask?.animator == null)
            return;

        currentMask.animator.SetTrigger(triggerName);
    }

    public void SwapMask(MaskState mask, CharacterProfile profile, bool force = false)
    {
        if (mask == null)
        {
            Debug.LogError("Cannot swap to null profile");
            return;
        }
        var maskIndex = GameManager.CurrentGameSave.Masks.IndexOf(mask);
        if (maskIndex < 0)
        {
            GameManager.CurrentGameSave.Masks.Add(mask);
            maskIndex = GameManager.CurrentGameSave.Masks.Count - 1;
        }

        if (!force && GameManager.CurrentGameSave.CurrentMask == maskIndex)
            return;

        GameManager.CurrentGameSave.CurrentMask = maskIndex;
        StartCoroutine(SwapAppearance(profile));
    }

    private IEnumerator SwapAppearance(CharacterProfile profile)
    {
        SwapMotor(null);
        if (loadedMasks == null)
            loadedMasks = new();

        //Play death animation of current animator
        if (currentMask != null)
            currentMask.animator.SetTrigger("Death");

        var newMaskInstance = loadedMasks.FirstOrDefault(m => m?.profile == profile);
        if (newMaskInstance == null)
        {
            newMaskInstance = new MaskInstance();
            newMaskInstance.profile = profile;
            newMaskInstance.instance = Instantiate(profile.prefab, transform, false);
            newMaskInstance.animator = newMaskInstance.instance.GetComponent<Animator>();
            loadedMasks.Add(newMaskInstance);
        }

        //Play death animation of new animator
        newMaskInstance.instance.gameObject.SetActive(true);
        newMaskInstance.animator.SetTrigger("Death");

        TendrilManager.SpreadTendrils();

        //Dissolve
        var t = 0f;
        while (t < 1)
        {
            if (currentMask != null)
                currentMask.instance.Material.SetFloat("_Dissolve", 1 - t);
            newMaskInstance.instance.Material.SetFloat("_Dissolve", t);

            t += Time.deltaTime;
            yield return null;
        }

        TendrilManager.ReleaseSpread();

        //Remove
        if (currentMask != null)
            currentMask.instance.gameObject.SetActive(false);

        newMaskInstance.instance.Material.SetFloat("_Dissolve", 1);
        newMaskInstance.animator.SetTrigger("Idle");
        currentMask = newMaskInstance;

        SwapMotor(profile.motor);
    }

    public void SwapMotor(EntityMotor newMotor)
    {
        currentMotor = newMotor;
    }

    private void EnableActions()
    {
        moveAction?.action?.Enable();
        jumpAction?.action?.Enable();
        crouchAction?.action?.Enable();

        if (pauseMenuAction?.action != null)
        {
            pauseMenuAction.action.performed -= OnPauseMenuAction;
            pauseMenuAction.action.performed += OnPauseMenuAction;
            pauseMenuAction.action.Enable();
        }

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

        if (pauseMenuAction?.action != null)
        {
            pauseMenuAction.action.performed -= OnPauseMenuAction;
            pauseMenuAction.action.Disable();
        }

        if (maskMenuAction?.action != null)
        {
            maskMenuAction.action.performed -= OnMaskMenuAction;
            maskMenuAction.action.Disable();
        }
    }

    private void OnPauseMenuAction(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (MenuManager.CurrentScreen == MenuManager.Screen.Pause)
            MenuManager.SetScreen(MenuManager.Screen.None);
        else if (MenuManager.CurrentScreen == MenuManager.Screen.Mask)
            MenuManager.SetScreen(MenuManager.Screen.None);
        else if (MenuManager.CurrentScreen == MenuManager.Screen.None)
            MenuManager.SetScreen(MenuManager.Screen.Pause);
    }

    private void OnMaskMenuAction(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        if (MenuManager.CurrentScreen == MenuManager.Screen.None)
            MenuManager.SetScreen(MenuManager.Screen.Mask);
        else if (MenuManager.CurrentScreen == MenuManager.Screen.Mask)
            MenuManager.SetScreen(MenuManager.Screen.None);
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
