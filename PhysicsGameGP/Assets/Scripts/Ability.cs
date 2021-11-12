using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Tooltip("If false, will activate OnShoot (on release of button)")]
    public bool activateOnAim;
    [Tooltip("Deactivate this ability when shooting again.")]
    public bool deactivateOnShot;
    
    public abstract void OnActivate();
    protected abstract void OnExecute();
    public abstract void OnDeactivate();
}

public enum Abilities
{
    Putt = 0,
    Blast = 1,
    Blink = 2,
    SlowMo = 3,
    Spike = 4,
}