using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D targetBody = default;
    [SerializeField] private Vector3 offset = default;

    [SerializeField] private float smoothTime = 0.1f;
    [SerializeField] private float forwardAmount = 1f;
    [Range(0.1f, 100f)][SerializeField] private float maxVelocity = 1f;

    private Vector3 velocity;

    private void FixedUpdate()
    {
        Vector3 targetPosition = (Vector3)targetBody.position + offset;

        float tmp = targetBody.velocity.magnitude / maxVelocity;
        float cameraTime = Mathf.Lerp(0f, forwardAmount, tmp);
        
        targetPosition += (Vector3)targetBody.velocity.normalized * cameraTime;
        
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
