using UnityEngine;

public class AbilitySpike : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;

    private bool isActive = false;
    private bool spikesActive = false;

    private float gravityScale;
    
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
        Vector2 force = playerController.force;
        body.AddForce(force, ForceMode2D.Impulse);
        
        isActive = true;
    }

    protected override void OnExecute()
    {
        Debug.Log("Spike OnExecute");
        spikesActive = true;
        gravityScale = body.gravityScale;
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
        Debug.Log("Spike OnCollisionEnter2D");
        if (isActive)
        {
            OnExecute();
        }
    }
}