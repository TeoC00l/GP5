using UnityEngine;

public abstract class Ability : MonoBehaviour
{
    [Tooltip("If false, will activate OnShoot (on release of button)")]
    public bool activateOnAim;
    
    public abstract void OnActivate();
    protected abstract void OnExecute();
    protected abstract void OnDeactivate();
}