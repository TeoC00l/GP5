using System.Collections;
using UnityEngine;

public class BreakablePlatform : MonoBehaviour
{
    [SerializeField] private int health = 1;
    [SerializeField] private float breakAfterTime = 1f;
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            health--;

            if (health <= 0)
            {
                if (breakAfterTime > 0f)
                    StartCoroutine(DieAfterTime());
                else
                    Die();
            }
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    
    private IEnumerator DieAfterTime()
    {
        yield return new WaitForSecondsRealtime(breakAfterTime);
        Die();
    }
}