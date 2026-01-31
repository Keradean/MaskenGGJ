using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

public class WaveSpawner : MonoBehaviour, ISpawner
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Waves")]
    [SerializeField] private List<Wave> waves;

    [Header("Wave Timing")]
    [SerializeField] private float timeBetweenWaves = 2f;

    private readonly Dictionary<GameObject, IObjectPool<GameObject>> pools = new();
    private readonly Dictionary<GameObject, IObjectPool<GameObject>> instancePoolMap = new();

    private int currentWaveIndex = 0;
    private int spawnedInCurrentWave = 0;
    private float nextSpawnTime = 0f;

    private bool waitingForNextWave = false;
    private float nextWaveStartTime = 0f;

    [System.Serializable]
    public class Wave
    {
        public GameObject enemyPrefab;
        public float timeBetweenSpawns = 1f;
        public int spawnAmount = 3;
    }

    private void Awake()
    {

    }

    private void Update()
    {
        if (waves == null || waves.Count == 0) return;
        if (currentWaveIndex >= waves.Count) return;

        if (waitingForNextWave)
        {
            if (Time.time < nextWaveStartTime) return;

            waitingForNextWave = false;
            NextWave();
            return;
        }

        Wave currentWave = waves[currentWaveIndex];

        if (spawnedInCurrentWave >= currentWave.spawnAmount)
        {
            StartWaveDelay();
            return;
        }

        if (Time.time < nextSpawnTime) return;

        GameObject prefab = currentWave.enemyPrefab;
        if (prefab == null)
        {
            StartWaveDelay();
            return;
        }

        var pool = GetPoolForPrefab(prefab);
        pool.Get();

        spawnedInCurrentWave++;
        nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
    }

    private void StartWaveDelay()
    {
        waitingForNextWave = true;
        nextWaveStartTime = Time.time + timeBetweenWaves;
    }

    private void NextWave()
    {
        currentWaveIndex++;
        spawnedInCurrentWave = 0;
        nextSpawnTime = Time.time;
    }

    #region Pool Management
    private IObjectPool<GameObject> GetPoolForPrefab(GameObject prefab)
    {
        if (pools.TryGetValue(prefab, out var existing)) return existing;

        IObjectPool<GameObject> pool = null;
        pool = new ObjectPool<GameObject>(
            createFunc: () => CreateEnemy(prefab, pool),
            actionOnGet: OnGet,
            actionOnRelease: OnRelease,
            actionOnDestroy: Destroy,
            collectionCheck: false,
            defaultCapacity: 0,
            maxSize: 100
        );

        pools[prefab] = pool;
        return pool;
    }

    private GameObject CreateEnemy(GameObject prefab, IObjectPool<GameObject> pool)
    {
        GameObject enemy = Instantiate(prefab);
        enemy.GetComponent<EnemyHealth>()?.SetSpawner(this);

        if (enemy != null && pool != null)
        {
            instancePoolMap[enemy] = pool;
        }

        enemy.SetActive(false); 
        return enemy;
    }

    private void OnGet(GameObject enemy)
    {
        if (enemy == null) return;

        enemy.SetActive(true);

        enemy.GetComponent<EnemyHealth>()?.ResetHealth();

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            enemy.transform.position = point.position;
            enemy.transform.rotation = Quaternion.identity;
        }
    }

    private void OnRelease(GameObject enemy)
    {
        if (enemy == null) return;
        enemy.SetActive(false);
    }

    public void ReturnToPool(GameObject enemy)
    {
        if (enemy == null) return;

        if (instancePoolMap.TryGetValue(enemy, out var pool))
        {
            pool.Release(enemy);
        }
        else
        {
            Destroy(enemy);
        }
    }
    #endregion
}
