using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private Rigidbody2D body;
    
    [SerializeField] private float maxForce = 10f;

    private bool shootRequested = false;

    private Vector2 clickPosition;

    private Vector2 dir;
    private float dragLength;
    [Tooltip("Max drag length multiplier by screen height")]
    [SerializeField] private float maxLength = 0.5f;
    [SerializeField] private bool clearForceOnShoot = false;
    [Tooltip("0f clears all the force, 1f keeps all the force")]
    [Range(0f,1f)]
    [SerializeField] private float clearForceAmount = 0f;

    private Vector2 force;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Input();
        
        if (shootRequested)
            Fire();
        
        
    }

    private void Input()
    {
        if (InputController.ShootRequested)
        {
            InputController.ShootRequested = false;
            
            clickPosition = InputController.LookVector;
        }
        
        if (InputController.ShootCancelled)
        {
            InputController.ShootCancelled = false;
            shootRequested = true;

            Vector2 dragPosition = InputController.LookVector;

            Vector2 startPos = clickPosition / Screen.height;
            Vector2 endPos = dragPosition / Screen.height;
            Vector2 v = startPos - endPos;

            dir = v.normalized;
            force = dir * Mathf.Lerp(0f, maxForce, v.magnitude / maxLength);
            force = Vector2.ClampMagnitude(force, maxForce);
        }
    }

    private void Fire()
    {
        shootRequested = false;

        if (clearForceOnShoot)
        {
            body.velocity = Vector3.Lerp(Vector3.zero, body.velocity, clearForceAmount);
        }
        
        body.AddForce(force, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(InputController.MouseWorldPoint, 5f);
        Gizmos.DrawLine(Vector3.zero, force);
    }
}
