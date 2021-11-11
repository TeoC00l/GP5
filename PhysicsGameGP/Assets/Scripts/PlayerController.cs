using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private Player player;
    private Rigidbody2D body;
    private LineRenderer[] lines;
    
    [SerializeField] private Transform spawnPosition = default;
    
    [SerializeField] private float maxForce = 10f;

    private bool shootRequested = false;

    private Vector2 clickPosition;

    private Vector2 dir;
    private float dragLength;
    [SerializeField] private float indicatorLengthMultiplier = 1f;
    [Tooltip("Max drag length multiplier by screen height")]
    [SerializeField] private float maxLength = 0.5f;
    [SerializeField] private bool clearForceOnShoot = false;
    [Tooltip("0f clears all the force, 1f keeps all the force")]
    [Range(0f,1f)]
    [SerializeField] private float clearForceAmount = 0f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundCheckMask = default;
    [SerializeField] private float maxGroundAngle = 45f;

    public Vector2 force;

    private bool isAiming;

    private Ability basicAbility;
    private Ability currentAbility;
    [SerializeField] private List<Ability> abilities = new List<Ability>();

    private int currentAbilityIndex = 0;

    private RaycastHit2D groundHit;
    [SerializeField] private float groundFriction = 0f;
    [Range(0f, 1f)][SerializeField] private float hitFriction = 0f;

    [SerializeField] private RandomWeight[] abilityDrops = default;

    private Queue<Abilities> abilityQueue = new Queue<Abilities>();
    public Queue<Abilities> GetAbilityQueue() { return abilityQueue; }

    [SerializeField] private int maxAirShotAmount = 1;
    private int currentAirShotAmount = 0;
    
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        lines = GetComponentsInChildren<LineRenderer>();
        
        basicAbility = GetComponent<AbilityPutt>();
        currentAbility = abilities[0];
    }

    private void Start()
    {
        abilityQueue.Enqueue(GetRandomAbility());
        abilityQueue.Enqueue(GetRandomAbility());
    }

    private void Update()
    {
        Debug.Log(abilityQueue.Count);
        
        Input();

        if (isAiming)
            VisualiseTrajectory();
        else
        {
            lines[0].gameObject.SetActive(false);
            lines[1].gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (GroundCheck())
            GroundFriction();
    }

    private void Input()
    {
        // LMB Down
        if (InputController.ShootRequested)
        {
            InputController.ShootRequested = false;
            isAiming = true;

            if (GroundCheck())
            {
                if (currentAbility.activateOnAim)
                {
                    currentAbility.OnActivate();
                }
            }
            else
            {
                if (currentAbility.activateOnAim)
                {
                    currentAbility.OnActivate();
                }
            }
            
            clickPosition = InputController.LookVector;
        }
        
        // LMB Up
        if (InputController.ShootCancelled)
        {
            InputController.ShootCancelled = false;
            isAiming = false;
            shootRequested = true;

            if (GroundCheck())
            {
                basicAbility.OnActivate();
            } else 
            {
                if (!currentAbility.activateOnAim)
                {
                    if (currentAirShotAmount < maxAirShotAmount)
                    {
                        currentAbility = GetAbilityByEnum(abilityQueue.Dequeue());
                        abilityQueue.Enqueue(GetRandomAbility());
                        
                        currentAirShotAmount++;
                        currentAbility.OnActivate();
                    }
                }
            }
        }

        if (InputController.NextAbility)
        {
            InputController.NextAbility = false;
            
            Debug.Log(currentAbilityIndex);
            
            currentAbility = abilities[currentAbilityIndex++];
            if (currentAbilityIndex >= abilities.Count)
                currentAbilityIndex = 0;
        }

        if (InputController.ResetRequested)
        {
            InputController.ResetRequested = false;

            body.MovePosition(spawnPosition.position);
            body.velocity = Vector2.zero;
        }
    }

    private void VisualiseTrajectory()
    {
        lines[0].gameObject.SetActive(true);
        lines[1].gameObject.SetActive(true);
        
        List<Vector3> trajectoryPoints = new List<Vector3>();
        List<Vector3> forcePoints = new List<Vector3>();
        
        Vector2 dragPosition = InputController.LookVector;

        Vector2 startPos = clickPosition / Screen.height;
        Vector2 endPos = dragPosition / Screen.height;
        Vector2 v = startPos - endPos;

        dir = v.normalized;
        force = dir * Mathf.Lerp(0f, maxForce, v.magnitude / maxLength);
        force = Vector2.ClampMagnitude(force, maxForce);
        trajectoryPoints.Add(transform.position);
        trajectoryPoints.Add(transform.position + (Vector3)force * indicatorLengthMultiplier);
        lines[0].SetPositions(trajectoryPoints.ToArray());
        
        
        forcePoints.Add(transform.position);
        forcePoints.Add(transform.position - (Vector3)force * indicatorLengthMultiplier);
        lines[1].SetPositions(forcePoints.ToArray());
        float tmp = Mathf.Lerp(0f, maxForce, force.magnitude / maxForce);
        lines[1].startWidth = tmp * 0.01f + 0.1f;
        lines[1].endWidth = tmp * 0.01f + 0.2f;
    }

    private void GroundFriction()
    {
        // Thanks to Wiktor Ravndal for helping with this mess
        Vector2 velocityPlaneProjection = Vector3.ProjectOnPlane(body.velocity, groundHit.normal);
        float speedAlongFloor = Vector2.Dot(body.velocity, velocityPlaneProjection.normalized);
        float friction = groundFriction * speedAlongFloor * speedAlongFloor;
        body.AddForce(-velocityPlaneProjection.normalized * friction);
        // float friction = minFriction + groundFriction * speedAlongFloor * speedAlongFloor;
        // friction = Mathf.Min(friction, speedAlongFloor);
        // body.velocity += -velocityPlaneProjection.normalized * friction;
    }

    private bool GroundCheck()
    {
        RaycastHit2D hit = Physics2D.CircleCast(body.position, groundCheckRadius, 
            Vector2.down, groundCheckDistance, groundCheckMask);
        if (hit && Mathf.Atan2(hit.normal.y, hit.normal.x) > maxGroundAngle * Mathf.Deg2Rad)
        {
            groundHit = hit;
            currentAirShotAmount = 0;
            return true;
        }
        return false;
    }

    private Abilities GetRandomAbility()
    {
        float total = 0f;
        foreach (var drop in abilityDrops)
        {
            total += drop.value;
        }

        // float total = abilityDrops[abilityDrops.Length - 1].value;
        
        float random = Random.Range(0f, total);
        Debug.Log("AbilityDrop: " + random);
        foreach (var drop in abilityDrops)
        {
            if (random <= drop.value)
            {
                return drop.ability;
            }
            random -= drop.value;
        }

        return Abilities.Putt;
    }

    private Ability GetAbilityByEnum(Abilities value)
    {
        Debug.Log(value);
        switch (value)
        {
            case Abilities.Putt:
                return GetComponent<AbilityPutt>();
            case Abilities.Blast:
                return GetComponent<AbilityBlast>();
            case Abilities.Blink:
                return GetComponent<AbilityBlink>();
            case Abilities.SlowMo:
                return GetComponent<AbilitySlowMo>();
            default:
                return GetComponent<AbilityPutt>();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(InputController.MouseWorldPoint, 5f);
        Gizmos.DrawLine(Vector3.zero, force);
    }

    [Serializable]
    struct RandomWeight
    {
        public Abilities ability;
        public float value;
    }

    public enum Abilities
    {
        Putt = 0,
        Blast = 1,
        Blink = 2,
        SlowMo = 3,
        
    }
}
