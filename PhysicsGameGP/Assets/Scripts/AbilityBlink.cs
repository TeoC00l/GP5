using UnityEngine;

public class AbilityBlink : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;
    private CircleCollider2D col;
    
    [SerializeField] private bool clearForceOnShoot = false;
    [Tooltip("0f clears all the force, 1f keeps all the force")]
    [Range(0f,1f)]
    [SerializeField] private float clearForceAmount = 0f;
    
    [SerializeField] private float range = 0f;
    [SerializeField] private bool blinkToLastMouseLocation = false;
    [SerializeField] private bool blinkToAimDirection = true;
    
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundCheckMask = default;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        col = GetComponent<CircleCollider2D>();
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
        Vector2 newPosition = body.position;
        if (blinkToLastMouseLocation)
        {
            // Blink
            Vector2 aimVector = (Vector2)InputController.MouseWorldPoint - body.position;
            
            if (aimVector.magnitude > range)
                newPosition = body.position + aimVector.normalized * range;
            else
                newPosition = InputController.MouseWorldPoint;
        }

        if (blinkToAimDirection)
        {
            // Blink
            Vector2 blinkDir = playerController.force.normalized;
            
            RaycastHit2D hit = Physics2D.CircleCast(body.position, groundCheckRadius, 
                blinkDir, range, groundCheckMask);
            
            newPosition = body.position + blinkDir * range;

            if (hit)
            {
                Debug.Log("Hit");
                newPosition = hit.point - blinkDir * col.radius;
            }
        }
        
        body.MovePosition(newPosition);
        
        // Clear Velocity
        if (clearForceOnShoot)
        {
            body.velocity = Vector3.Lerp(Vector3.zero, body.velocity, clearForceAmount);
        }
        
        // Shoot
        Vector2 force = playerController.force;
        body.AddForce(force, ForceMode2D.Impulse);
    }

    public override void OnDeactivate()
    {
        
    }
}