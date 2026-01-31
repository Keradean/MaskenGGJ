using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ActionChase : FSMAction
{
    [Header("Config")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float modelRotationOffset = 0f; // 0, 90, 180, 270

    [Header("Movement")]
    [SerializeField] private bool usePhysics = true;

    private EnemyBrain enemyBrain;
    private Rigidbody rb;

    private void Awake()
    {
        enemyBrain = GetComponent<EnemyBrain>();
        rb = GetComponent<Rigidbody>();

        if (rb != null && usePhysics)
        {
            rb.freezeRotation = true;
        }
    }

    public override void Act()
    {
        ChasePlayer();
    }

    private void ChasePlayer()
    {
        if (enemyBrain.Player == null) return;

        // Richtung zum Player (nur horizontal)
        Vector3 dirToPlayer = enemyBrain.Player.position - transform.position;
        dirToPlayer.y = 0;

        float distanceToPlayer = dirToPlayer.magnitude;

        // ROTATION zum Player (mit Model Offset)
        if (dirToPlayer.sqrMagnitude > 0.01f)
        {
            // Base Rotation
            Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer);

            // Model Offset hinzufügen
            targetRotation *= Quaternion.Euler(0f, modelRotationOffset, 0f);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotationSpeed
            );
        }

        // MOVEMENT (nur wenn weiter weg als stopDistance)
        if (distanceToPlayer >= stopDistance)
        {
            if (usePhysics && rb != null)
            {
                // Physik-basierte Bewegung
                Vector3 moveDirection = dirToPlayer.normalized;
                Vector3 targetVelocity = moveDirection * chaseSpeed;
                targetVelocity.y = rb.linearVelocity.y; // Y für Gravity beibehalten

                rb.linearVelocity = targetVelocity;
            }
            else
            {
                // Transform-basierte Bewegung
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    transform.position + dirToPlayer.normalized,
                    chaseSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            // Stoppen wenn zu nah am Player
            if (usePhysics && rb != null)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity.x = 0;
                velocity.z = 0;
                rb.linearVelocity = velocity;
            }
        }
    }
}