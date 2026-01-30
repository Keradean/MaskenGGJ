using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActionWander : FSMAction
{
    [Header("Movement Config")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float wanderTime = 5f;
    [SerializeField] private Vector3 moveRange = new Vector3(10f, 0f, 10f);
    [SerializeField] private bool useGravity = true;
    [SerializeField] private bool stickToGround = true;
    [SerializeField] private float arrivalDistance = 0.5f;
    [SerializeField] private LayerMask groundLayer = 1;

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
            Debug.LogError($"[{gameObject.name}] ActionWander: Kein Rigidbody gefunden!");
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
            rb.useGravity = useGravity;
            // WICHTIG: Verhindere dass die Sphere umkippt
            rb.freezeRotation = true;

            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] ActionWander initialisiert:");
                Debug.Log($"  - Start Position: {startPosition}");
                Debug.Log($"  - Speed: {speed}");
                Debug.Log($"  - Gravity: {useGravity}");
                Debug.Log($"  - Rigidbody Mass: {rb.mass}");
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
            Debug.LogError($"[{gameObject.name}] ActionWander.Act(): Rigidbody ist null!");
            return;
        }

        timer -= Time.deltaTime;

        Vector3 moveDirection = (movePosition - transform.position).normalized;
        Vector3 horizontalMovement = moveDirection * speed;

        MoveWithRigidbody(horizontalMovement);

        float distanceToTarget = Vector3.Distance(transform.position, movePosition);

        if (showDebugLogs && Time.frameCount % 60 == 0) // Jede Sekunde (bei 60 FPS)
        {
            Debug.Log($"[{gameObject.name}] Wander Status:");
            Debug.Log($"  - Position: {transform.position}");
            Debug.Log($"  - Ziel: {movePosition}");
            Debug.Log($"  - Distanz: {distanceToTarget:F2}m");
            Debug.Log($"  - Velocity: {rb.linearVelocity}");
            Debug.Log($"  - Timer: {timer:F1}s");
        }

        if (distanceToTarget <= arrivalDistance || timer <= 0f)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] Neues Ziel wird gesetzt! (Distanz: {distanceToTarget:F2}, Timer: {timer:F1})");
            }
            GetNewDestination();
            timer = wanderTime;
        }
    }

    private void MoveWithRigidbody(Vector3 moveVelocity)
    {
        Vector3 targetVelocity = moveVelocity;

        if (stickToGround)
        {
            // Bewege nur horizontal, Y bleibt wie aktuell
            targetVelocity.y = rb.linearVelocity.y;
        }
        else if (!useGravity)
        {
            // Gravity deaktiviert -> Y konstant
            targetVelocity.y = 0f;
        }

        rb.linearVelocity = targetVelocity;
    }

    private void GetNewDestination()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-moveRange.x, moveRange.x),
            Random.Range(-moveRange.y, moveRange.y),
            Random.Range(-moveRange.z, moveRange.z)
        );

        Vector3 newPosition = startPosition + randomOffset;

        if (stickToGround)
        {
            newPosition = EnsurePositionOnGround(newPosition);
        }

        movePosition = newPosition;

        if (showDebugLogs)
        {
            Debug.Log($"[{gameObject.name}] Neues Ziel gesetzt: {movePosition}");
        }
    }

    private Vector3 EnsurePositionOnGround(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            position.y = hit.point.y + 0.1f; // kleine Höhe über Boden
        }
        else
        {
            position.y = startPosition.y;
        }
        return position;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;

        // Bewegungsbereich (blau)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, moveRange * 2f);

        if (Application.isPlaying)
        {
            // Aktuelles Ziel (rot)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(movePosition, 0.5f);
            Gizmos.DrawLine(transform.position, movePosition);

            // Startposition (grün)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPosition, 0.3f);
        }

        // Aktuelle Position (weiß)
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}