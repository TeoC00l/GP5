using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Tooltip("Deactivate this ability when shooting.")]
    [HideInInspector] public bool deactivateOnShot;
    [Tooltip("Deactivate this ability when shooting again after shooting once.")]
    [HideInInspector] public bool deactivateOnNextShot;

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
    Deadweight = 5,
    Platform = 6,
}