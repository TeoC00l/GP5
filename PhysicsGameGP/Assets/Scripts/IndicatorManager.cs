using System;
using System.Collections.Generic;
using UnityEngine;

public class IndicatorManager : MonoBehaviour
{
    private LineRenderer[] lines;

    [SerializeField] private float indicatorLengthMultiplier = 1f;

    private void Awake()
    {
        lines = GetComponentsInChildren<LineRenderer>();
    }

    public void DisableIndicators()
    {
        lines[0].gameObject.SetActive(false);
        lines[1].gameObject.SetActive(false);
    }

    public void VisualisePower(Vector2 force, float maxForce)
    {
        lines[0].gameObject.SetActive(true);
        
        List<Vector3> forcePoints = new List<Vector3>();

        forcePoints.Add(transform.position);
        forcePoints.Add(transform.position - (Vector3)force * indicatorLengthMultiplier);
        lines[0].SetPositions(forcePoints.ToArray());
        float tmp = Mathf.Lerp(0f, maxForce, force.magnitude / maxForce);
        lines[0].startWidth = tmp * 0.01f + 0.1f;
        lines[0].endWidth = tmp * 0.01f + 0.2f;
    }
    
    public void VisualiseTrajectory(Vector2 force)
    {
        lines[1].gameObject.SetActive(true);
        
        List<Vector3> trajectoryPoints = new List<Vector3>();
        
        trajectoryPoints.Add(transform.position);
        trajectoryPoints.Add(transform.position + (Vector3)force * indicatorLengthMultiplier);
        lines[1].SetPositions(trajectoryPoints.ToArray());
    }
}