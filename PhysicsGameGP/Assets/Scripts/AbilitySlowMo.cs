using System.Collections;
using UnityEngine;

public class AbilitySlowMo : Ability
{
    [SerializeField] private float slowMoSpeed = 0.2f;
    [SerializeField] private float timeLength = 2f;
    
    public override void OnActivate()
    {
        OnExecute();
        StartCoroutine(DeactivateAfterTime());
    }

    protected override void OnExecute()
    {
        Time.timeScale = 0.2f;
        Time.fixedDeltaTime = Time.fixedDeltaTime * Time.timeScale;
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