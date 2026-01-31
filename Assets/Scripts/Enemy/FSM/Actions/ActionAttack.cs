using UnityEngine;

public class ActionAttack : FSMAction
{
    [Header("Config")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float timeBtwAttacks = 1.5f;

    [Header("Rotation")]
    [SerializeField] private bool rotateToPlayer = true;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float modelRotationOffset = 0f; // 0, 90, 180, 270

    private EnemyBrain enemyBrain;
    private float timer;

    private void Awake()
    {
        enemyBrain = GetComponent<EnemyBrain>();
    }

    public override void Act()
    {
        AttackPlayer();
    }

    private void AttackPlayer()
    {
        if (enemyBrain.Player == null) return;

        // ROTATION: Schaue zum Player während Attack!
        if (rotateToPlayer)
        {
            Vector3 directionToPlayer = enemyBrain.Player.position - transform.position;
            directionToPlayer.y = 0; // Nur horizontal

            if (directionToPlayer.sqrMagnitude > 0.01f)
            {
                // Base Rotation zum Player
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

                // Model Rotation Offset hinzufügen (falls Model rückwärts schaut)
                targetRotation *= Quaternion.Euler(0f, modelRotationOffset, 0f);

                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeed
                );
            }
        }

        // ATTACK TIMER
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            IDamageable player = enemyBrain.Player.GetComponent<IDamageable>();

            if (player != null)
            {

                // Damage machen
                player.TakeDamage(damage);

                Debug.Log($"[{gameObject.name}] Attacked Player for {damage} damage!");
            }

            timer = timeBtwAttacks;
        }
    }
}