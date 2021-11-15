using UnityEngine;

public class PickupAbility : MonoBehaviour
{
    [SerializeField] private Abilities ability;
    [SerializeField] private bool destroyOnPickup = true;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().AddAbility(ability);
            
            if (destroyOnPickup)
                Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}