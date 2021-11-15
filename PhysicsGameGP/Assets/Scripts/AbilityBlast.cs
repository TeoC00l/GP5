using System;
using UnityEngine;

public class AbilityBlast : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;
    
    [SerializeField] private float blastMultiplier = 2f;

    [Tooltip("0f clears all the force, 1f keeps all the force")]
    [Range(0f,1f)]
    [SerializeField] private float clearForceAmount = 0f;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    public override void OnAim()
    {
        
    }

    public override void OnShoot()
    {
        OnExecute();
    }

    protected override void OnExecute()
    {
        // Clear Velocity
        if (clearForceAmount != 0f)
        {
            body.velocity = Vector3.Lerp(Vector3.zero, body.velocity, clearForceAmount);
        }
        
        // Shoot
        Vector2 force = playerController.force;
        force *= blastMultiplier;
        body.AddForce(force, ForceMode2D.Impulse);
    }

    public override void OnDeactivate()
    {
        
    }
}