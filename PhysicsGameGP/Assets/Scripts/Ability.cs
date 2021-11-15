using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Tooltip("Deactivate this ability when shooting again.")]
    public bool deactivateOnShot;

    public abstract void OnAim();
    public abstract void OnShoot();
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