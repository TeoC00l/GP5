using UnityEngine;

public class AbilityBlink : Ability
{

    public override void OnActivate()
    {
        Debug.Log("Blink");
    }

    protected override void OnExecute()
    {
        
    }

    protected override void OnDeactivate()
    {
        
    }
}