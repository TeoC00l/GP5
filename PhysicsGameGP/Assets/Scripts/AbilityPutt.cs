using UnityEngine;

public class AbilityPutt : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;
    
    [SerializeField] private bool clearForceOnShoot = false;
    [Tooltip("0f clears all the force, 1f keeps all the force")]
    [Range(0f,1f)]
    [SerializeField] private float clearForceAmount = 0f;
    
    public override void OnActivate()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        OnExecute();
    }

    protected override void OnExecute()
    {
        Vector2 force = playerController.force;
        
        if (clearForceOnShoot)
        {
            body.velocity = Vector3.Lerp(Vector3.zero, body.velocity, clearForceAmount);
        }
        
        body.AddForce(force, ForceMode2D.Impulse);
    }

    protected override void OnDeactivate()
    {
        throw new System.NotImplementedException();
    }
}