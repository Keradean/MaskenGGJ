using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActionWanderFlying : FSMAction
{
    [Header("Movement Config")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float wanderTime = 5f;
    [SerializeField] private Vector3 moveRange = new Vector3(10f, 5f, 10f); 
    [SerializeField] private float arrivalDistance = 0.5f;

    [Header("Flying Settings")]
    [SerializeField] private bool smoothMovement = true; 
    [SerializeField] private float smoothSpeed = 2f; 

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private Vector3 movePosition;
    private float timer;
    private Vector3 startPosition;
    private Rigidbody rb;
    private bool isInitialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError($"[{gameObject.name}] ActionWanderFlying: Kein Rigidbody gefunden!");
        }
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        startPosition = transform.position;

        if (rb != null)
        {
            
            rb.useGravity = false; 
            rb.freezeRotation = true; 
            rb.linearDamping = 1f; 

            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] ActionWanderFlying initialisiert:");
                Debug.Log($"  - Start Position: {startPosition}");
                Debug.Log($"  - Speed: {speed}");
                Debug.Log($"  - Move Range: {moveRange}");
                Debug.Log($"  - Smooth Movement: {smoothMovement}");
            }
        }

        GetNewDestination();
        timer = wanderTime;
        isInitialized = true;
    }

    public override void Act()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        if (rb == null)
        {
            Debug.LogError($"[{gameObject.name}] ActionWanderFlying.Act(): Rigidbody ist null!");
            return;
        }

        timer -= Time.deltaTime;

        // Richtung zum Ziel 
        Vector3 moveDirection = (movePosition - transform.position).normalized;

        if (smoothMovement)
        {
            // Sanfte Bewegung 
            Vector3 targetVelocity = moveDirection * speed;
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, Time.deltaTime * smoothSpeed);
        }
        else
        {
            // Direkte Bewegung
            rb.linearVelocity = moveDirection * speed;
        }

        float distanceToTarget = Vector3.Distance(transform.position, movePosition);

        if (showDebugLogs && Time.frameCount % 60 == 0) 
        {
            Debug.Log($"[{gameObject.name}] Flying Status:");
            Debug.Log($"  - Position: {transform.position}");
            Debug.Log($"  - Ziel: {movePosition}");
            Debug.Log($"  - Distanz: {distanceToTarget:F2}m");
            Debug.Log($"  - Velocity: {rb.linearVelocity}");
            Debug.Log($"  - Timer: {timer:F1}s");
        }

        // Neues Ziel wenn angekommen oder Zeit abgelaufen
        if (distanceToTarget <= arrivalDistance || timer <= 0f)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] Neues Flugziel wird gesetzt!");
            }
            GetNewDestination();
            timer = wanderTime;
        }
    }

    private void GetNewDestination()
    {
        // Zufällige Position
        Vector3 randomOffset = new Vector3(
            Random.Range(-moveRange.x, moveRange.x),
            Random.Range(-moveRange.y, moveRange.y),
            Random.Range(-moveRange.z, moveRange.z)
        );

        movePosition = startPosition + randomOffset;

        if (showDebugLogs)
        {
            Debug.Log($"[{gameObject.name}] Neues Flugziel: {movePosition} (Offset: {randomOffset})");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        // Bewegungsbereich 
        Gizmos.color = new Color(0, 0.5f, 1f, 0.3f);
        Gizmos.DrawWireCube(center, moveRange * 2f);
        Gizmos.DrawCube(center, moveRange * 2f);

        if (Application.isPlaying && isInitialized)
        {
            // Aktuelles Ziel 
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(movePosition, 0.5f);
            Gizmos.DrawLine(transform.position, movePosition);

            // Startposition 
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPosition, 0.4f);

            // Bewegungsrichtung
            Gizmos.color = Color.yellow;
            Vector3 direction = (movePosition - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direction * 2f);
        }

        // Aktuelle Position
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}