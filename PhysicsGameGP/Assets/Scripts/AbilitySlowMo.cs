using System.Collections;
using UnityEngine;

public class AbilitySlowMo : Ability
{
    private Rigidbody2D body;
    private PlayerController playerController;
    
    [SerializeField] private bool deactivateAfterTime = false;
    [SerializeField] private float slowMoSpeed = 0.2f;
    [SerializeField] private float timeLength = 2f;
    
    [SerializeField] private bool clearForceOnShoot = false;
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
        OnExecute();
    }

    public override void OnShoot()
    {
        if (deactivateAfterTime)
            StartCoroutine(DeactivateAfterTime());
    }

    protected override void OnExecute()
    {
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = Time.fixedDeltaTime * Time.timeScale;
        
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
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    private IEnumerator DeactivateAfterTime()
    {
        yield return new WaitForSecondsRealtime(timeLength);
        OnDeactivate();
    }
}