using UnityEngine;

public class AbilitySpike : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;

    private bool isActive = false;
    private bool spikesActive = false;
    
    public override void OnActivate()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        Vector2 force = playerController.force;
        body.AddForce(force, ForceMode2D.Impulse);
        
        isActive = true;
    }

    protected override void OnExecute()
    {
        Debug.Log("Spike OnExecute");
        spikesActive = true;
        body.isKinematic = true;
        body.velocity = Vector2.zero;
        playerController.ResetCurrentAirShotAmount();
    }

    public override void OnDeactivate()
    {
        isActive = false;
        spikesActive = false;
        body.isKinematic = false;
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
    
    // private void FixedUpdate()
    // {
    //     if (spikesActive)
    //         body.velocity = Vector2.zero;
    // }
}