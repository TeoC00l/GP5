using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.U2D;
using Vector2 = UnityEngine.Vector2;

[RequireComponent(typeof(AreaEffector2D))]
[RequireComponent(typeof(BoxCollider2D))]

public class Wind : MonoBehaviour
{
    [SerializeField] private float windMagnitude;
    [SerializeField] private float pauseTimeInSeconds;
    [SerializeField] private float runTimeInSeconds;

    private float range;
    private float height;
    private AreaEffector2D effector;
    private BoxCollider2D boxCollider;
    private LineRenderer boxLineRenderer;
    private LineRenderer directionLineRenderer;

    private Vector2 intersectionPosition;
    private Vector2 start;

    
    private bool hitPlayer;
    private float currentWindMagnitude;
    
    private Vector2[] intersectionPoints;

    private void OnValidate()
    {
        if (boxCollider)
        {
            return;
        }
        
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.usedByEffector = true;
        boxCollider.isTrigger = true;
        
        
    }

    void Start()
    {
        Vector2 size = GetComponent<BoxCollider2D>().size;
        range = size.x;
        height = size.y;
        
        LineRenderer[] renderers = GetComponentsInChildren<LineRenderer>();
        boxLineRenderer = renderers[0];
        directionLineRenderer = renderers[1];
        boxLineRenderer.loop = true;
        boxLineRenderer.startWidth = 0.2f;
        boxLineRenderer.endWidth = 0.2f;
        directionLineRenderer.startColor = Color.red;
        directionLineRenderer.endColor = Color.green;
        directionLineRenderer.startWidth = 0.6f;
        directionLineRenderer.endWidth = 0.1f;
        
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

    private void Update()
    {
        DrawDebugLines();

    }

    void DrawDebugLines()
    {
        float degrees = effector.forceAngle;
        float radians = degrees * Mathf.Deg2Rad;

        Vector2 dir = (new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized);
        Vector2 start = ( (Vector2) transform.localPosition ) + boxCollider.offset;
        Vector2 end = (Vector2) transform.localPosition + (dir * range);

        directionLineRenderer.positionCount = 2;
        directionLineRenderer.SetPosition(0, start);
        directionLineRenderer.SetPosition(1, end);

        Vector2[] corners = CalculateCornerPoints();

        boxLineRenderer.positionCount = corners.Length;

        for (int i = 0; i < corners.Length; i++)
        {
            boxLineRenderer.SetPosition(i, corners[i]);
        }

        Vector2[] intersectionPoints = new Vector2[4];
        float[] pointDistances = new float[4];

        //Get intersection points
        for (int i = 0; i < intersectionPoints.Length; i++)
        {
            if (i+1 < intersectionPoints.Length)
            {
                intersectionPoints[i] = FindIntersectionPoint(start, end, corners[i], corners[i+1]);
            }
            else
            {
                intersectionPoints[i] = FindIntersectionPoint(start, end, corners[i], corners[0]);
            }
        }

        this.start = start;
        this.intersectionPoints = intersectionPoints;
        this.intersectionPosition = FindClosestPoint(start, dir, intersectionPoints);
    }

    Vector2[] CalculateCornerPoints()
    {
        Vector2 size = boxCollider.size;
        Vector2 center = (Vector2) transform.localPosition + boxCollider.offset;

        float top = center.y + size.y / 2f;
        float btm = center.y - size.y / 2f;
        float left = center.x - size.x / 2f;
        float right = center.x + size.x / 2f;
        
        Vector2[] corners = new Vector2[4];
        
        corners[0] = new Vector2(right, top);
        corners[1] = new Vector2(right, btm);
        corners[2] = new Vector2(left, btm);
        corners[3] = new Vector2(left, top);

        return corners;
    }

    Vector2 FindIntersectionPoint(Vector2 l1start, Vector2 l1end, Vector2 l2start, Vector2 l2end)
    {
        //get direction of vectors
        Vector2 l1dir = (l1end - l1start).normalized;
        Vector2 l2dir = (l2end - l2start).normalized;
        
        //get normal of vectors
        Vector2 l1normal = new Vector2(-l1dir.y, l1dir.x);
        Vector2 l2normal = new Vector2(-l2dir.y, l2dir.x);
        
        //rewrite lines to general form
        float A = l1normal.x;
        float B = l1normal.y;

        float C = l2normal.x;
        float D = l2normal.y;

        float k1 = (A * l1start.x) + (B * l1start.y);
        float k2 = (C * l2start.x) + (D * l2start.y);

        if (IsParallel(l1normal, l2normal))
        {
            return Vector2.zero;
        }

        //Step 4: calculate the intersection point -> one solution
        float xIntersect = (D * k1 - B * k2) / (A * D - B * C);
        float yIntersect = (-C * k1 + A * k2) / (A * D - B * C);
        
        Vector2 intersectPoint = new Vector2(xIntersect, yIntersect);

        return intersectPoint;
    }

    Vector2 FindClosestPoint(Vector2 start, Vector2 dir, Vector2[] points)
    {
        Vector2 closestPoint = Vector2.zero;
        float closestPointDistance = 0f;

        for (int i = 0; i < intersectionPoints.Length ; i++)
        {
            if (intersectionPoints[i] == Vector2.zero)
            {
                continue;
            }

            if (closestPoint == Vector2.zero)
            {
                closestPoint = intersectionPoints[i];
                closestPointDistance = Vector2.Distance(intersectionPoints[i], start);
                continue;
            }

            float distance = Vector2.Distance(intersectionPoints[i], start);

            if (distance <= closestPointDistance)
            {
                closestPoint = intersectionPoints[i];
                closestPointDistance = distance;
            }
        }

        intersectionPosition = closestPoint;
        
        return intersectionPosition;
    }

    Vector2 FindForwardIntersection(Vector2 ForwardVector, Vector2[] points)
    {
        Vector2 biggestDot = Vector2.zero;
        float thatDot = 0f;
        
        for (int i = 0; i < intersectionPoints.Length; i++)
        {
            float dot = Vector2.Dot(ForwardVector.normalized, points[i].normalized);

            if (biggestDot == Vector2.zero)
            {
                biggestDot = intersectionPoints[i];
                thatDot = dot;
                continue;
            }

            if (dot < thatDot)
            {
                biggestDot = intersectionPoints[i];
                thatDot = dot;
                continue;
            }
        }

        return biggestDot;
    }

    bool IsParallel(Vector2 v1, Vector2 v2)
    {
        //2 vectors are parallel if the angle between the vectors are 0 or 180 degrees
        if (Vector2.Angle(v1, v2) == 0f || Vector2.Angle(v1, v2) == 180f)
        {
            return true;
        }

        return false;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (intersectionPoints == null)
        {
            return;
        }
        
        foreach(Vector2 intersectionpoint in intersectionPoints)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(intersectionpoint, 0.2f);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(intersectionPosition, 0.6f);
        
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(start, 0.6f);

    }
#endif

}
