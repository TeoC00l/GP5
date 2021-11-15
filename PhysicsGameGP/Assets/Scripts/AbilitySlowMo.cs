using System.Collections;
using UnityEngine;

public class AbilitySlowMo : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;
    
    [SerializeField] private bool deactivateAfterTime = false;
    [SerializeField] private float slowMoSpeed = 0.2f;
    [SerializeField] private float timeLength = 2f;
    
    [Tooltip("0f clears all the force, 1f keeps all the force")]
    [Range(0f,1f)]
    [SerializeField] private float clearForceAmount = 0f;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        deactivateOnShot = true;
    }

    public override void OnAim()
    {
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = Time.fixedDeltaTime * Time.timeScale;
    }

    public override void OnShoot()
    { 
        OnExecute();
        if (deactivateAfterTime)
            StartCoroutine(DeactivateAfterTime());
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
        body.AddForce(force, ForceMode2D.Impulse);
    }

    public override void OnDeactivate()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSecondsRealtime(timeLength);
        OnDeactivate();
    }
}