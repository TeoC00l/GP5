using System.Collections;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Transform endPositionTransform;
    [SerializeField] private float speed;
    [SerializeField] private float pauseTimeInSeconds;
    private float step;

    private Vector2 startPosition;
    private Vector2 endPosition;

    private bool movingForward;
    private bool pause;

    void Start()
    {
        startPosition = transform.position;
        endPosition = endPositionTransform.position;

        movingForward = true;
        
        step = speed * Time.deltaTime;
    }

    void Update()
    {
        if (pause)
        {
            return;
        }
        
        if (movingForward)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPosition, step);

            if (Vector2.Distance(transform.position, endPosition) < 0.3f)
            {
                movingForward = false;
                pause = true;

                StartCoroutine(OnPause());
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, startPosition, step);
            
            if (Vector2.Distance(transform.position, startPosition) < 0.3f)
            {
                movingForward = true;
                pause = true;

                StartCoroutine(OnPause());
            }
        }
    }

    IEnumerator OnPause()
    {
        yield return new WaitForSeconds(pauseTimeInSeconds);

        pause = false;
    }
}


