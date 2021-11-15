using System;
using UnityEngine;

public class AbilityPlatform : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;
    
    [SerializeField] private float range = 1f;
    
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundCheckMask = default;

    [SerializeField] private GameObject platformPrefab = default;
    
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
        Vector2 pos = body.position;
        
        // Blink
        Vector2 dir = playerController.force.normalized;
            
        RaycastHit2D hit = Physics2D.CircleCast(body.position, groundCheckRadius, 
            dir, range, groundCheckMask);
            
        pos = body.position + dir * range;

        if (hit)
        {
            pos = hit.point - dir;
        }

        Instantiate(platformPrefab, pos, Quaternion.identity);
    }

    public override void OnDeactivate()
    {
        
    }
}