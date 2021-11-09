using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    private Player player;
    private Rigidbody2D body;
    private LineRenderer[] lines;
    
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

    private bool isAiming;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        lines = GetComponentsInChildren<LineRenderer>();
    }

    private void Update()
    {
        Input();
        
        if (shootRequested)
            Fire();

        if (isAiming)
            VisualiseTrajectory();
        else
        {
            lines[0].gameObject.SetActive(false);
            lines[1].gameObject.SetActive(false);
        }
    }

    private void Input()
    {
        if (InputController.ShootRequested)
        {
            InputController.ShootRequested = false;
            isAiming = true;
            player.OnStartAiming();
            
            clickPosition = InputController.LookVector;
        }
        
        if (InputController.ShootCancelled)
        {
            InputController.ShootCancelled = false;
            isAiming = false;
            shootRequested = true;
            player.OnShoot();

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

    private void VisualiseTrajectory()
    {
        lines[0].gameObject.SetActive(true);
        lines[1].gameObject.SetActive(true);
        
        List<Vector3> trajectoryPoints = new List<Vector3>();
        List<Vector3> forcePoints = new List<Vector3>();
        
        Vector2 dragPosition = InputController.LookVector;

        Vector2 startPos = clickPosition / Screen.height;
        Vector2 endPos = dragPosition / Screen.height;
        Vector2 v = startPos - endPos;

        dir = v.normalized;
        force = dir * Mathf.Lerp(0f, maxForce, v.magnitude / maxLength);
        force = Vector2.ClampMagnitude(force, maxForce);
        trajectoryPoints.Add(transform.position);
        trajectoryPoints.Add(transform.position + (Vector3)force);
        lines[0].SetPositions(trajectoryPoints.ToArray());
        
        
        forcePoints.Add(transform.position);
        forcePoints.Add(transform.position - (Vector3)force);
        lines[1].SetPositions(forcePoints.ToArray());
        float tmp = Mathf.Lerp(0f, maxForce, force.magnitude / maxForce);
        lines[1].startWidth = tmp * 0.01f + 0.1f;
        lines[1].endWidth = tmp * 0.01f + 0.2f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(InputController.MouseWorldPoint, 5f);
        Gizmos.DrawLine(Vector3.zero, force);
    }
}
