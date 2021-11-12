using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour, Controls.IGameplayActions
{
    private Controls controls;
    private Camera cam;

    private static InputController instance;

    public static Vector3 MouseWorldPoint;
    public static Vector2 LookVector;
    public static bool ShootRequested;
    public static bool ShootCancelled;
    public static bool NextAbility;
    public static bool ResetRequested;
    
    private void Awake()
    {
        controls = new Controls();
        controls.Gameplay.SetCallbacks(this);
        instance = this;
            
        cam = Camera.main;
    }

    private void OnEnable()
    {
        controls.Enable();
        // SubscribeToInput();
    }

    private void OnDisable()
    {
        controls.Disable();
        // UnsubscribeFromInput();
    }

    private void Update()
    {
        Vector3 mouseWorldPoint = cam.ScreenToWorldPoint(new Vector3(LookVector.x, LookVector.y, 0f));
        MouseWorldPoint = mouseWorldPoint;
    }
    //
    // private void OnShootPerformed(InputAction.CallbackContext ctx)
    // {
    //     ShootRequested = true;
    // }
    //
    // private void OnShootCancelled(InputAction.CallbackContext ctx)
    // {
    //     ShootCancelled = true;
    // }
    //
    // private void OnNextAbilityPerformed(InputAction.CallbackContext ctx)
    // {
    //     NextAbility = true;
    // }
    //
    // private void OnResetPerformed(InputAction.CallbackContext ctx)
    // {
    //     ResetRequested = true;
    // }
    //
    // #region Internal Subscription
    // private void SubscribeToInput()
    // {
    //     controls.Gameplay.Shoot.performed += OnShootPerformed;
    //     controls.Gameplay.Shoot.canceled += OnShootCancelled;
    //     controls.Gameplay.NextAbility.performed += OnNextAbilityPerformed;
    //     controls.Gameplay.Reset.performed += OnResetPerformed;
    // }
    //     
    // private void UnsubscribeFromInput()
    // {
    //     controls.Gameplay.Shoot.performed -= OnShootPerformed;
    //     controls.Gameplay.Shoot.canceled -= OnShootCancelled;
    //     controls.Gameplay.NextAbility.performed -= OnNextAbilityPerformed;
    //     controls.Gameplay.Reset.performed -= OnResetPerformed;
    // }
    // #endregion

    private void LateUpdate()
    {
        ConsumeInputs();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
            ShootRequested = true;
        else if (context.canceled)
            ShootCancelled = true;
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        LookVector = context.ReadValue<Vector2>();
    }

    public void OnNextAbility(InputAction.CallbackContext context)
    {
        
    }

    public void OnReset(InputAction.CallbackContext context)
    {
        if (context.performed)
            ResetRequested = true;
    }

    public void ConsumeInputs()
    {
        ShootRequested = false;
        ShootCancelled = false;
        ResetRequested = false;
    }
}