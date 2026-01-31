using UnityEngine;

// ==================================================
// ENEMY HEALTH CLASS
// ==================================================
// Manages health and death
// Works with or without pooling system
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Config")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private PlayerStats playerStats; 

    public float CurrentHealth { get; private set; }

    private EnemyBrain enemyBrain;
    private ISpawner spawner; 

    private void Awake()
    {
        enemyBrain = GetComponent<EnemyBrain>();
    }

    private void Start()
    {
        ResetHealth();
    }

    // ==================================================
    // SET SPAWNER - Called by Spawner
    // ==================================================
    public void SetSpawner(ISpawner spawner)
    {
        this.spawner = spawner;
    }

    // ==================================================
    // TAKE DAMAGE
    // ==================================================
    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount;

        Debug.Log($"[{gameObject.name}] Took {amount} damage! Health: {CurrentHealth}/{maxHealth}");

        if (CurrentHealth <= 0f)
        {
            Die();
        }
    }

    // ==================================================
    // DIE
    // ==================================================
    private void Die()
    {
        Debug.Log($"Stirb du eleneder!!!");

        if(playerStats != null)
        {
            playerStats.AddScore(playerStats.EnemyApeScore);
            Debug.Log($"Her mit den Moneten");
        }
        // Disable AI
        if (enemyBrain != null)
        {
            enemyBrain.enabled = false;
        }

        // Return to pool OR destroy
        if (spawner != null)
        {
            // Spawned from pool - return to pool
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            // Manual enemy (not from pool) - destroy
            Destroy(gameObject);
        }
    }

    // ==================================================
    // RESET HEALTH - Called when respawned
    // ==================================================
    public void ResetHealth()
    {
        CurrentHealth = maxHealth;

        if (enemyBrain != null)
        {
            enemyBrain.enabled = true;
        }

        Debug.Log($"[{gameObject.name}] Health reset to {CurrentHealth}");
    }
}