using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.U2D;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(AreaEffector2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Wind : MonoBehaviour
{
    [SerializeField] private bool usePause;
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
    private CircleCollider2D player;

    private Vector2 back;
    private Vector2 front;
    private float frontToBackDistance;
    private Vector2[] intersectionPoints;
    private float cachedMagnitude;
    private bool blocked;
    private bool isBlowing;

#if UNITY_EDITOR
    private void OnValidate()
    {
        OnStart();
    }
#endif

    void OnStart()
    {
        if (!boxCollider)
        {
            boxCollider = GetComponent<BoxCollider2D>();
            boxCollider.usedByEffector = true;
            boxCollider.isTrigger = true;
        }

        Vector2 size = GetComponent<BoxCollider2D>().size;
        range = size.x;
        height = size.y;

        LineRenderer[] renderers = GetComponentsInChildren<LineRenderer>();
        boxLineRenderer = renderers[0];
        directionLineRenderer = renderers[1];
        boxLineRenderer.loop = true;
        Color lineColor = Color.white;
        lineColor.a = 0.3f;
        boxLineRenderer.endColor = lineColor;
        boxLineRenderer.startColor = lineColor;
        boxLineRenderer.startWidth = 0.05f;
        boxLineRenderer.endWidth = 0.05f;
        Color endColor = Color.cyan;
        endColor.a = 0.03f;
        directionLineRenderer.startColor = Color.cyan;
        directionLineRenderer.endColor = endColor;
        directionLineRenderer.startWidth = size.y;
        directionLineRenderer.endWidth = size.y;

        effector = GetComponent<AreaEffector2D>();
        effector.forceAngle = 0;

        hitPlayer = false;


        CalculateCornerPoints();
        DrawDebugLines();
    }

    void Start()
    {
        OnStart();
        cachedMagnitude = effector.forceMagnitude;
        StartCoroutine(Blow());
    }

    IEnumerator Blow()
    {
        isBlowing = true;
        float timeActive = 0.0f;
        DrawDebugLines();

        while (timeActive < runTimeInSeconds)
        {
            if (usePause)
            {
                timeActive += Time.deltaTime;
            }

            yield return null;
        }

        StartCoroutine(OnPause());
    }

    IEnumerator OnPause()
    {
        isBlowing = false;

        ClearRenderers();
        effector.forceMagnitude = 0f;

        yield return new WaitForSeconds(pauseTimeInSeconds);

        StartCoroutine(Blow());
    }

    void DrawDebugLines()
    {
        float degrees = transform.rotation.eulerAngles.z;
        float radians = degrees * Mathf.Deg2Rad;

        Vector2 dir = (new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized);
        Vector2 start = (Vector2) boxCollider.bounds.center;
        Vector2 end = (Vector2) start + (dir * range);

        this.start = start;

        directionLineRenderer.positionCount = 2;
        directionLineRenderer.SetPosition(0, back);
        directionLineRenderer.SetPosition(1, front);

        Vector2[] corners = CalculateCornerPoints();

        boxLineRenderer.positionCount = corners.Length;

        for (int i = 0; i < corners.Length; i++)
        {
            boxLineRenderer.SetPosition(i, corners[i]);
        }

        Vector2[] intersectionPoints = new Vector2[4];

        //Get intersection points
        for (int i = 0; i < intersectionPoints.Length; i++)
        {
            if (i + 1 < intersectionPoints.Length)
            {
                intersectionPoints[i] = FindIntersectionPoint(start, end, corners[i], corners[i + 1]);
            }
            else
            {
                intersectionPoints[i] = FindIntersectionPoint(start, end, corners[i], corners[0]);
            }
        }

        this.start = start;
        this.intersectionPoints = intersectionPoints;
        Vector2[] closestPoints = FindClosestPoint((boxCollider.bounds.center), dir, intersectionPoints);
        FindForwardIntersection(dir, closestPoints, start);
    }

    Vector2[] CalculateCornerPoints()
    {
        Vector2 size = boxCollider.size;
        Vector2 center = (Vector2) boxCollider.bounds.center;

        float top = 0 + (size.y / 2f);
        float btm = 0 - (size.y / 2f);
        float left = 0 - (size.x / 2f);
        float right = 0 + (size.x / 2f);

        Vector2[] corners = new Vector2[4];

        Vector3 vec = new Vector2(right, top);
        Quaternion rotation = transform.rotation;
        vec = rotation * vec;
        vec += (Vector3) center;
        corners[0] = (Vector2) vec;

        vec = new Vector2(right, btm);
        vec = rotation * vec;
        vec += (Vector3) center;
        corners[1] = vec;

        vec = new Vector2(left, btm);
        vec = rotation * vec;
        vec += (Vector3) center;
        corners[2] = vec;


        vec = new Vector2(left, top);
        vec = rotation * vec;
        vec += (Vector3) center;
        corners[3] = vec;

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

    Vector2[] FindClosestPoint(Vector2 start, Vector2 dir, Vector2[] points)
    {
        Vector2[] closestPoints = new Vector2[2];
        float closestPointDistance = 0f;

        for (int i = 0; i < intersectionPoints.Length; i++)
        {
            if (intersectionPoints[i] == Vector2.zero)
            {
                continue;
            }

            if (closestPoints[0] == Vector2.zero)
            {
                closestPoints[0] = intersectionPoints[i];
                closestPointDistance = Vector2.Distance(intersectionPoints[i], start);
                continue;
            }

            float distance = Vector2.Distance(intersectionPoints[i], start);
            float dif = Mathf.Abs(distance - closestPointDistance);

            if (dif > 0.05f && distance < closestPointDistance)
            {
                closestPoints[0] = intersectionPoints[i];
                closestPointDistance = Vector2.Distance(intersectionPoints[i], start);
            }
            else if (dif < 0.05f)
            {
                closestPoints[1] = intersectionPoints[i];
            }
        }


        return closestPoints;
    }

    void FindForwardIntersection(Vector2 ForwardVector, Vector2[] points, Vector2 start)
    {
        Vector2 toTarget = (points[0] - start).normalized;

        if (Vector2.Dot(ForwardVector, toTarget) > 0)
        {
            front = points[0];
            back = points[1];
        }
        else
        {
            front = points[1];
            back = points[0];
        }

        frontToBackDistance = Vector2.Distance(front, back);
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

    private void Update()
    {
        blocked = IsBlocked();
        
        if (player)
        {
            if (boxCollider.IsTouching(player))
            {
                SetMagnitude(player);
            }
            else
            {
                effector.forceMagnitude = 0;
            }
        }
    }

    void DrawIntersectionPoints()
    {
        if (intersectionPoints == null)
        {
            return;
        }

        foreach (Vector2 intersectionpoint in intersectionPoints)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(intersectionpoint, 0.2f);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(front, 0.6f);

        Vector2 center = boxCollider.bounds.center;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(center, 1f);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(Vector3.zero, 1f);


        Gizmos.color = Color.red;
        Gizmos.DrawSphere(back, 0.6f);

        Gizmos.color = Color.white;

        for (int i = 0; i < intersectionPoints.Length; i++)
        {
            if (i == intersectionPoints.Length - 1)
            {
                Gizmos.DrawLine(intersectionPoints[i], intersectionPoints[0]);
                break;
            }

            Gizmos.DrawLine(intersectionPoints[i], intersectionPoints[i + 1]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.CompareTag("Player"))
        {
            blocked = IsBlocked();
            
            if (!player)
            {
                player = other.GetComponent<CircleCollider2D>();
            }
        
            SetMagnitude(other);
        }
    }

    private void SetMagnitude(Collider2D other)
    {
        if (blocked)
        {
            Debug.Log("blocked");
            effector.forceMagnitude = 0;
            return;
        }
        else
        {
            Debug.Log("NOT BLOCKED");

        }
        
        if (!isBlowing)
        {
            return;
        }

        Vector2 position = other.bounds.center;
        float distance = Vector2.Distance(position, front);
        float t = distance / frontToBackDistance;
        float strength = Mathf.Lerp((cachedMagnitude * 0.1f), cachedMagnitude, t);
        effector.forceMagnitude = strength;
    }

    private void ClearRenderers()
    {
        boxLineRenderer.positionCount = 0;
        directionLineRenderer.positionCount = 0;
    }

    bool IsBlocked()
    {
        //Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 2;
        layerMask = ~layerMask;

        RaycastHit2D hit;
        Vector2 size = new Vector2(boxCollider.size.y, boxCollider.size.x/30);
        Vector2 forwardVector = (front - back).normalized;
        Vector2 origin = back + (forwardVector * size.y);
        Vector2 end = front - (forwardVector * (size.y * 2));



        hit = Physics2D.BoxCast(origin, size, transform.eulerAngles.z +90f, forwardVector,
            frontToBackDistance-(size.y*2), layerMask);

        Debug.DrawLine(origin, hit.point, Color.red);

        if (hit)
        {
            
            Debug.Log(hit.transform.name);
            if (hit.transform.CompareTag("Player"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            Debug.Log("No hit");
        }

        return false;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawIntersectionPoints();
    }
#endif
}