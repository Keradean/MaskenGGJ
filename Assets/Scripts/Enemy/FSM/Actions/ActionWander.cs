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

    [Header("Rotation")]
    [SerializeField] private bool rotateTowardsMovement = true;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float modelRotationOffset = 0f; // ← NEU! 0, 90, 180, 270

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

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
            rb.freezeRotation = true;

            if (showDebugLogs)
            {
                Debug.Log($"[{gameObject.name}] ActionWander initialisiert:");
                Debug.Log($"  - Model Rotation Offset: {modelRotationOffset}°");
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

        // ROTATION: Drehe dich in Bewegungsrichtung (mit Offset für Model)
        if (rotateTowardsMovement && moveDirection.sqrMagnitude > 0.01f)
        {
            Vector3 horizontalDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);

            if (horizontalDirection.sqrMagnitude > 0.01f)
            {
                // Base Rotation zur Bewegungsrichtung
                Quaternion targetRotation = Quaternion.LookRotation(horizontalDirection);

                // Model Rotation Offset hinzufügen
                targetRotation *= Quaternion.Euler(0f, modelRotationOffset, 0f);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        // MOVEMENT
        Vector3 horizontalMovement = moveDirection * speed;
        MoveWithRigidbody(horizontalMovement);

        float distanceToTarget = Vector3.Distance(transform.position, movePosition);

        if (distanceToTarget <= arrivalDistance || timer <= 0f)
        {
            GetNewDestination();
            timer = wanderTime;
        }
    }

    private void MoveWithRigidbody(Vector3 moveVelocity)
    {
        Vector3 targetVelocity = moveVelocity;

        if (stickToGround)
        {
            targetVelocity.y = rb.linearVelocity.y;
        }
        else if (!useGravity)
        {
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
    }

    private Vector3 EnsurePositionOnGround(Vector3 position)
    {
        if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            position.y = hit.point.y + 0.1f;
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

        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, moveRange * 2f);

        if (Application.isPlaying && isInitialized)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(movePosition, 0.5f);
            Gizmos.DrawLine(transform.position, movePosition);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPosition, 0.3f);

            if (rotateTowardsMovement)
            {
                Gizmos.color = Color.yellow;
                Vector3 direction = (movePosition - transform.position).normalized;
                Gizmos.DrawRay(transform.position, direction * 2f);
            }
        }

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}