using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    private Player player;
    private Rigidbody2D body;
    private Rigidbody2D connectedBody;
    private Rigidbody2D previousConnectedBody;
    private IndicatorManager indicatorManager;

    private bool isConnectedToBody = false;
    private bool ignoreConnectedBodyMovement = false;
    private float connectedBodyIgnoreTime = 0.2f; // Hardcoded
    
    [Header("Respawn")]
    [SerializeField] private Transform spawnPosition = default;
    
    [Header("Controller")]
    [SerializeField] private float maxForce = 10f;

    private Vector2 clickPosition;
    private Vector2 dir;
    [HideInInspector] public Vector2 force;
    private float dragLength;
    
    private bool isAiming;
    
    [Tooltip("Max drag length multiplier by screen height")]
    [SerializeField] private float maxLength = 0.5f;
    
    [SerializeField] private int maxAirShotAmount = 1;
    private int currentAirShotAmount = 0;
    public void ResetCurrentAirShotAmount() { currentAirShotAmount = 0; }
    [SerializeField] private bool lockInputUntilAtRest = true;
    [SerializeField] private float maxRestingVelocity = 0.5f;
    private bool waitingToRest = false;
    [SerializeField] private float collisionIgnoreTime = 0.1f;
    private bool ignoreCollision = false;

    [Header("Visuals")] 
    [SerializeField] private Material playerMaterial; 
    [SerializeField] private Material playerlockedMaterial;

    private Renderer playerRenderer;
    
    [Header("Ground Check")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask groundCheckMask = default;
    [SerializeField] private float maxGroundAngle = 45f;
    
    private RaycastHit2D groundHit;
    private bool isTempGrounded = false;
    public void SetIsTempGrounded(bool value) { isTempGrounded = value; }
    
    [Header("Friction")]
    [SerializeField] private float groundFriction = 0f;
    
    [Header("Abilities")]
    [SerializeField] private bool useRandomAbilitySystem = false;
    [SerializeField] private int abilitiesInQueueAmount = 2;
    [SerializeField] private RandomWeight[] abilityDrops = default;

    private Queue<Abilities> abilityQueue = new Queue<Abilities>();
    public Queue<Abilities> GetAbilityQueue() { return abilityQueue; }

    private Ability basicAbility;
    private Ability currentAbility;
    private Ability previousAbility;
    
    private Vector2 connectionWorldPosition;
    private Vector2 connectionVelocity;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        indicatorManager = GetComponent<IndicatorManager>();
        playerRenderer = GetComponentInChildren<Renderer>();
        
        basicAbility = GetComponent<AbilityPutt>();
        currentAbility = basicAbility;
        previousAbility = basicAbility;
    }

    private void Start()
    {
        if (useRandomAbilitySystem)
        {
            for (int i = 0; i < abilitiesInQueueAmount; i++)
            {
                abilityQueue.Enqueue(GetRandomAbility());
            }
        }
    }

    private void Update()
    {
        if (lockInputUntilAtRest)
        {
            if (RestingCheck() && GroundCheck() ||
                !waitingToRest && currentAirShotAmount < maxAirShotAmount && !GroundCheck() ||
                isConnectedToBody && GroundCheck())
            {
                playerRenderer.material = playerMaterial;
                Input();
                CalculateInput();
            }
            else
            {
                isAiming = false;
                playerRenderer.material = playerlockedMaterial;
            }
        }
        else
        {
            Input();
            CalculateInput();
        }

        if (isAiming)
        {
            indicatorManager.VisualisePower(force, maxForce);
            indicatorManager.VisualiseTrajectory(force);
        }
        else
        {
            indicatorManager.DisableIndicators();
        }
        
        if (InputController.ResetRequested)
        {
            body.MovePosition(spawnPosition.position);
            body.velocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (GroundCheck())
        {
            if (!ignoreConnectedBodyMovement)
                ConnectedBodyMovement(); 
            else
                ClearConnectedBody();
            GroundFriction();
        }
        else
        {
            previousConnectedBody = null;
            connectionVelocity = Vector2.zero;
            connectionWorldPosition = Vector2.zero;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            if (!ignoreCollision)
                waitingToRest = true;
        }
    }

    private void Input()
    {
        // LMB Down
        if (InputController.ShootRequested)
        {
            isAiming = true;
            
            if (GroundCheck())
                basicAbility.OnAim();
            else
            {
                if (currentAirShotAmount < maxAirShotAmount)
                {
                    currentAbility = GetAbilityByEnum(
                        abilityQueue.TryDequeue(out Abilities ability) ? ability : Abilities.Putt);

                    if (useRandomAbilitySystem)
                        abilityQueue.Enqueue(GetRandomAbility());
                    
                    currentAbility.OnAim();
                }
            }

            clickPosition = InputController.LookVector;
        }
        
        // LMB Up
        if (InputController.ShootCancelled)
        {
            isAiming = false;

            if (currentAbility.deactivateOnShot)
                currentAbility.OnDeactivate();
            if (previousAbility.deactivateOnNextShot)
                previousAbility.OnDeactivate();
            previousAbility = currentAbility;
            
            if (GroundCheck())
            {
                isTempGrounded = false;
                StartCoroutine(ConnectedBodyIgnoreTimer());
                StartCoroutine(InputLockCollisionIgnoreTimer());
                basicAbility.OnShoot();
            } else if (currentAirShotAmount < maxAirShotAmount)
            {
                currentAirShotAmount++;
                isTempGrounded = false;
                StartCoroutine(ConnectedBodyIgnoreTimer());
                StartCoroutine(InputLockCollisionIgnoreTimer());
                currentAbility.OnShoot();
            }
        }
    }

    private void CalculateInput()
    {
        Vector2 dragPosition = InputController.LookVector;
        
        Vector2 startPos = clickPosition / Screen.height;
        Vector2 endPos = dragPosition / Screen.height;
        Vector2 v = startPos - endPos;
        
        dir = v.normalized;
        force = dir * Mathf.Lerp(0f, maxForce, v.magnitude / maxLength);
        force = Vector2.ClampMagnitude(force, maxForce);
    }
    
    private void GroundFriction()
    {
        // Thanks to Wiktor Ravndal for helping with this mess
        Vector2 velocityPlaneProjection = Vector3.ProjectOnPlane(body.velocity, groundHit.normal);
        float speedAlongFloor = Vector2.Dot(body.velocity, velocityPlaneProjection.normalized);
        float friction = groundFriction * speedAlongFloor * speedAlongFloor;
        float floorAngleDot = Vector2.Dot(groundHit.normal, Vector2.up); // Reduce friction on slopes
        float angleMultiplier = floorAngleDot <= 0f ? 1f : floorAngleDot; // Reduce friction on slopes
        body.AddForce(friction * angleMultiplier * -velocityPlaneProjection.normalized);
        // body.AddForce(friction * -velocityPlaneProjection.normalized);
        
        // float friction = minFriction + groundFriction * speedAlongFloor * speedAlongFloor;
        // friction = Mathf.Min(friction, speedAlongFloor);
        // body.velocity += -velocityPlaneProjection.normalized * friction;
    }

    private void ConnectedBodyMovement()
    {
        if (Vector2.Dot(groundHit.normal, Vector2.up) <= maxGroundAngle * Mathf.Rad2Deg &&
            !groundHit.collider.gameObject.TryGetComponent(out connectedBody))
        {
            ClearConnectedBody();
            return;
        }
        
        if (connectedBody)
        {
            if (connectedBody.isKinematic || connectedBody.mass >= body.mass)
            {
                UpdateConnectionState();
            }
        }
    }

    private void ClearConnectedBody()
    {
        previousConnectedBody = null;
        connectionVelocity = Vector2.zero;
        connectionWorldPosition = Vector2.zero;
        isConnectedToBody = false;
    }

    private void UpdateConnectionState()
    {
        if (connectedBody == previousConnectedBody)
        {
            isConnectedToBody = true;
            
            Vector3 connectionMovement =
                connectedBody.position - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = connectedBody.position;
        
        Vector2 relativeVelocity = body.velocity - connectionVelocity;
        float currentX = Vector2.Dot(relativeVelocity, Vector2.left);
        body.velocity += new Vector2(currentX, 0f);
        previousConnectedBody = connectedBody;
    }

    private bool GroundCheck()
    {
        if (isTempGrounded)
        {
            currentAirShotAmount = 0; 
            return true;
        }
        
        RaycastHit2D hit = Physics2D.CircleCast(body.position, groundCheckRadius, 
            Vector2.down, groundCheckDistance, groundCheckMask);
        if (hit && Mathf.Atan2(hit.normal.y, hit.normal.x) > maxGroundAngle * Mathf.Deg2Rad)
        {
            groundHit = hit;
            if (!lockInputUntilAtRest)
                currentAirShotAmount = 0;
            
            return true;
        }
        return false;
    }

    private bool RestingCheck()
    {
        //TODO: if wallstuck by using spike groundcheck will be false, fix
        if (!(GroundCheck() || isTempGrounded))
            if ((isConnectedToBody ? body.velocity.magnitude - connectionVelocity.magnitude : body.velocity.magnitude) 
                >= maxRestingVelocity)
                return false;
        
        waitingToRest = false;
        currentAirShotAmount = 0;
        return true;
    }

    private Abilities GetRandomAbility()
    {
        float total = 0f;
        foreach (var drop in abilityDrops)
        {
            total += drop.value;
        }

        float random = Random.Range(0f, total);
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
            case Abilities.Spike:
                return GetComponent<AbilitySpike>();
            case Abilities.Deadweight:
                return GetComponent<AbilityDeadweight>();
            case Abilities.Platform:
                return GetComponent<AbilityPlatform>();
            default:
                return GetComponent<AbilityPutt>();
        }
    }

    public void AddAbility(Abilities value)
    {
        abilityQueue.Enqueue(value);
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
    
    // Ignore collision for a certain amount of time after shooting the first shot to prevent
    // locking input if a wall is hit in the first couple of frames
    private IEnumerator InputLockCollisionIgnoreTimer()
    {
        ignoreCollision = true;
        yield return new WaitForSecondsRealtime(collisionIgnoreTime);
        ignoreCollision = false;
    }

    private IEnumerator ConnectedBodyIgnoreTimer()
    {
        ignoreConnectedBodyMovement = true;
        yield return new WaitForSecondsRealtime(connectedBodyIgnoreTime);
        ignoreConnectedBodyMovement = false;
    }
}

// private void VisualiseTrajectory()
// {
//     lines[0].gameObject.SetActive(true);
//     lines[1].gameObject.SetActive(true);
//     
//     List<Vector3> trajectoryPoints = new List<Vector3>();
//     List<Vector3> forcePoints = new List<Vector3>();
//     
//     Vector2 dragPosition = InputController.LookVector;
//     
//     Vector2 startPos = clickPosition / Screen.height;
//     Vector2 endPos = dragPosition / Screen.height;
//     Vector2 v = startPos - endPos;
//     
//     dir = v.normalized;
//     force = dir * Mathf.Lerp(0f, maxForce, v.magnitude / maxLength);
//     force = Vector2.ClampMagnitude(force, maxForce);
//     trajectoryPoints.Add(transform.position);
//     trajectoryPoints.Add(transform.position + (Vector3)force * indicatorLengthMultiplier);
//     lines[0].SetPositions(trajectoryPoints.ToArray());
//     
//     
//     forcePoints.Add(transform.position);
//     forcePoints.Add(transform.position - (Vector3)force * indicatorLengthMultiplier);
//     lines[1].SetPositions(forcePoints.ToArray());
//     float tmp = Mathf.Lerp(0f, maxForce, force.magnitude / maxForce);
//     lines[1].startWidth = tmp * 0.01f + 0.1f;
//     lines[1].endWidth = tmp * 0.01f + 0.2f;
// }
