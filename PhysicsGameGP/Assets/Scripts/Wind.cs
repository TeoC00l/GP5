using System.Collections;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(AreaEffector2D))]
public class Wind : MonoBehaviour
{
    [SerializeField] private float windMagnitude;
    [SerializeField] private float pauseTimeInSeconds;
    [SerializeField] private float runTimeInSeconds;

    private float range;
    private float height;
    private AreaEffector2D effector;
    
    private bool hitPlayer;
    private float currentWindMagnitude;
    
    
    void Start()
    {
        Vector2 size = GetComponent<BoxCollider2D>().size;
        range = size.x;
        height = size.y;
        
        effector = GetComponent<AreaEffector2D>();
        hitPlayer = false;
        StartCoroutine(Blow());
    }

    bool CheckForPlayer()
    {
        RaycastHit2D hit;

        float degrees = effector.forceAngle;
        float radians = degrees * Mathf.Deg2Rad;
        
        Vector2 dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        Vector2 end = (Vector2) transform.position + (dir * range);
        //Vector2 end = (Vector2) transform.position + (Vector2.right * 100f);
        Debug.DrawLine((Vector2) transform.position, end , Color.red, 0.1f, false);

        return false;
    }

    IEnumerator Blow()
    {
        float timeActive = 0.0f;
        


        effector.forceMagnitude = windMagnitude;

        while (timeActive < runTimeInSeconds)
        {
            CheckForPlayer();

            timeActive += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(OnPause());
    }

    IEnumerator OnPause()
    {
        effector.forceMagnitude = 0f;

        yield return new WaitForSeconds(pauseTimeInSeconds);
        
        StartCoroutine(Blow());
    }

}
