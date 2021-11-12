using UnityEngine;

public class AbilityBlast : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;
    [SerializeField] private float blastMultiplier = 2f;
    
    public override void OnActivate()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        
        OnExecute();
    }

    protected override void OnExecute()
    {
        Vector2 force = playerController.force;

        force *= blastMultiplier;
        
        body.AddForce(force, ForceMode2D.Impulse);
    }

    public override void OnDeactivate()
    {
        
    }
}