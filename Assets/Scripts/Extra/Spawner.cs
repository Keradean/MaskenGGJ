using UnityEngine;
using UnityEngine.Pool;

// ==================================================
// SPAWNER CLASS
// ==================================================
// Spawns GameObjects using Object Pooling for performance
// Works independently - just spawns and pools GameObjects
public class Spawner : MonoBehaviour, ISpawner
{
    // ==================================================
    // VARIABLE DECLARATION - SPAWN CONFIGURATION
    // ==================================================

    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float timeBtwSpawns;
    [SerializeField] private int spawnStop;

    private float timeSinceLastSpawn;
    private int currentSpawnCount;

    // ==================================================
    // VARIABLE DECLARATION - OBJECT POOLING
    // ==================================================

    // GameObject prefab to spawn (can be any enemy prefab)
    [SerializeField] private GameObject enemyPrefab;

    // Object pool for GameObjects
    private IObjectPool<GameObject> enemyPool;

    // ==================================================
    // AWAKE METHOD
    // ==================================================
    private void Awake()
    {
        enemyPool = new ObjectPool<GameObject>(CreateEnemy, OnGet, OnRelease);
        currentSpawnCount = 0;
    }

    // ==================================================
    // ON GET METHOD (POOL CALLBACK)
    // ==================================================
    private void OnGet(GameObject enemy)
    {
        // Activate GameObject
        enemy.SetActive(true);

        // Reset health / AI state when reused
        enemy.GetComponent<EnemyHealth>()?.ResetHealth();

        // Random spawn point
        Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        enemy.transform.position = randomSpawnPoint.position;
    }

    // ==================================================
    // ON RELEASE METHOD (POOL CALLBACK)
    // ==================================================
    private void OnRelease(GameObject enemy)
    {
        // Deactivate GameObject
        enemy?.SetActive(false);
    }

    // ==================================================
    // CREATE ENEMY METHOD (POOL CALLBACK)
    // ==================================================
    private GameObject CreateEnemy()
    {
        // Instantiate new GameObject
        GameObject enemy = Instantiate(enemyPrefab);

        // Tell enemy about this spawner (so it can return to pool)
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.SetSpawner(this);
        }

        return enemy;
    }

    // ==================================================
    // UPDATE METHOD
    // ==================================================
    public void Update()
    {
        if (currentSpawnCount >= spawnStop) return;

        if (Time.time > timeSinceLastSpawn)
        {
            enemyPool.Get();
            timeSinceLastSpawn = Time.time + timeBtwSpawns;
            currentSpawnCount++;
        }
    }

    // ==================================================
    // PUBLIC METHOD - RETURN TO POOL
    // ==================================================
    // EnemyHealth calls this when enemy dies
    public void ReturnToPool(GameObject enemy)
    {
        enemyPool.Release(enemy);
    }
}