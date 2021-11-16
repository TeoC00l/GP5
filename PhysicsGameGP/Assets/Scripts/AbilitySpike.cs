using UnityEngine;

public class AbilitySpike : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;

    private bool isActive = false;
    private bool spikesActive = false;

    private float gravityScale;
    
    [Tooltip("0f clears all the force, 1f keeps all the force")]
    [Range(0f,1f)]
    [SerializeField] private float clearForceAmount = 0f;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        gravityScale = body.gravityScale;

        deactivateOnNextShot = true;
    }

    public override void OnAim()
    {
        
    }

    public override void OnShoot()
    {
        // Clear Velocity
        if (clearForceAmount != 0f)
        {
            body.velocity = Vector3.Lerp(Vector3.zero, body.velocity, clearForceAmount);
        }
        
        // Shoot
        Vector2 force = playerController.force;
        body.AddForce(force, ForceMode2D.Impulse);
        
        isActive = true;
    }

    protected override void OnExecute()
    {
        spikesActive = true;
        body.gravityScale = 0f;
        body.velocity = Vector2.zero;
        playerController.ResetCurrentAirShotAmount();
    }

    public override void OnDeactivate()
    {
        isActive = false;
        spikesActive = false;
        body.gravityScale = gravityScale;
        // Remove visuals when player shoots again
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isActive)
        {
            OnExecute();
        }
    }
}