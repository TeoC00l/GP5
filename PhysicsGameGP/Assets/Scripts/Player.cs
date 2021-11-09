using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private PlayerController playerController;
    
    public void OnStartAiming()
    {
        // SlowMotion (TimeScale)
    }

    public void OnShoot()
    {
        // Blink (controller, rigidbody)
        // Spikeball (Enable OnCollisionEnter, rigidbody)
        // Deadweight (Disable OnCollisionEnter, rigidbody)
        // Freeze ball (rigidbody, Disable next shot)
        // Reverse Gravity (rigidbody)
        // Change the size (collider, mesh)
        // Powerful shot (controller, rigidbody)
    }
}