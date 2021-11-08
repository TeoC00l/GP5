using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private Controls controls;
    private Camera cam;

    private static InputController instance;

    public static Vector3 MouseWorldPoint;
    public static Vector2 LookVector => instance.controls.Gameplay.MousePosition.ReadValue<Vector2>();
    public static bool ShootRequested;
    public static bool ShootCancelled;
        
    private void Awake()
        {
            controls = new Controls();
            instance = this;
            
            cam = Camera.main;
        }

        private void OnEnable()
        {
            controls.Enable();
            SubscribeToInput();
        }

        private void OnDisable()
        {
            controls.Disable();
            UnsubscribeFromInput();
        }

        private void Update()
        {
            Vector3 mouseWorldPoint = cam.ScreenToWorldPoint(new Vector3(LookVector.x, LookVector.y, 0f));
            MouseWorldPoint = mouseWorldPoint;
        }

        private void OnShootPerformed(InputAction.CallbackContext ctx)
        {
            ShootRequested = true;
        }

        private void OnShootCancelled(InputAction.CallbackContext ctx)
        {
            ShootCancelled = true;
        }

        #region Internal Subscription
        private void SubscribeToInput()
        {
            controls.Gameplay.Shoot.performed += OnShootPerformed;
            controls.Gameplay.Shoot.canceled += OnShootCancelled;
        }
        
        private void UnsubscribeFromInput()
        {
            controls.Gameplay.Shoot.performed -= OnShootPerformed;
            controls.Gameplay.Shoot.canceled -= OnShootCancelled;
        }
        #endregion
    }