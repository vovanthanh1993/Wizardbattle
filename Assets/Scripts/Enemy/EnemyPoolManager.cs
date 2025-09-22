using UnityEngine;
using System.Collections.Generic;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [System.Serializable]
    public class EnemyPrefabData
    {
        public GameObject enemyPrefab;
        public int poolSize = 20;
        public float spawnWeight = 1f; // Higher weight = more likely to spawn
        public string enemyName = "Enemy";
    }

    [Header("Enemy Pool Settings")]
    [SerializeField] private EnemyPrefabData[] _enemyPrefabs;
    [SerializeField] private int _defaultPoolSize = 20;

    [Header("Spawn Settings")]
    [SerializeField] private float _minSpawnDistance = 5f;
    [SerializeField] private float _maxSpawnDistance = 15f;

    [Header("Pool Parent")]
    [SerializeField] private Transform _poolParent;

    private Dictionary<GameObject, ObjectPool<EnemyController>> _enemyPools;
    private Dictionary<GameObject, List<EnemyController>> _activeEnemies;
    private List<GameObject> _spawnablePrefabs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        _enemyPools = new Dictionary<GameObject, ObjectPool<EnemyController>>();
        _activeEnemies = new Dictionary<GameObject, List<EnemyController>>();
        _spawnablePrefabs = new List<GameObject>();

        // Initialize pools for each enemy prefab
        foreach (var enemyData in _enemyPrefabs)
        {
            if (enemyData.enemyPrefab != null)
            {
                var enemy = enemyData.enemyPrefab.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    int poolSize = enemyData.poolSize > 0 ? enemyData.poolSize : _defaultPoolSize;
                    _enemyPools[enemyData.enemyPrefab] = new ObjectPool<EnemyController>(enemy, poolSize, _poolParent);
                    _activeEnemies[enemyData.enemyPrefab] = new List<EnemyController>();
                    
                    // Add to spawnable prefabs if weight > 0
                    if (enemyData.spawnWeight > 0)
                    {
                        _spawnablePrefabs.Add(enemyData.enemyPrefab);
                    }
                }
            }
        }
    }

    // Enemy Pool Methods
    public EnemyController GetEnemy()
    {
        return GetRandomEnemy();
    }

    public EnemyController GetRandomEnemy()
    {
        if (_spawnablePrefabs.Count == 0) return null;

        // Get random prefab based on weight
        GameObject selectedPrefab = GetWeightedRandomPrefab();
        if (selectedPrefab == null) return null;

        return GetEnemy(selectedPrefab);
    }

    public EnemyController GetEnemy(GameObject prefab)
    {
        if (!_enemyPools.ContainsKey(prefab)) return null;

        EnemyController enemy = _enemyPools[prefab].Get();
        if (enemy != null)
        {
            _activeEnemies[prefab].Add(enemy);
        }
        return enemy;
    }

    public void ReturnEnemy(EnemyController enemy)
    {
        if (enemy == null) return;

        // Find which prefab this enemy belongs to
        foreach (var kvp in _enemyPools)
        {
            if (enemy.gameObject.name.Contains(kvp.Key.name))
            {
                kvp.Value.Return(enemy);
                _activeEnemies[kvp.Key].Remove(enemy);
                break;
            }
        }
    }

    private GameObject GetWeightedRandomPrefab()
    {
        if (_spawnablePrefabs.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var prefab in _spawnablePrefabs)
        {
            var enemyData = GetEnemyData(prefab);
            if (enemyData != null)
            {
                totalWeight += enemyData.spawnWeight;
            }
        }

        if (totalWeight <= 0) return _spawnablePrefabs[0];

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var prefab in _spawnablePrefabs)
        {
            var enemyData = GetEnemyData(prefab);
            if (enemyData != null)
            {
                currentWeight += enemyData.spawnWeight;
                if (randomValue <= currentWeight)
                {
                    return prefab;
                }
            }
        }

        return _spawnablePrefabs[0];
    }

    private EnemyPrefabData GetEnemyData(GameObject prefab)
    {
        foreach (var enemyData in _enemyPrefabs)
        {
            if (enemyData.enemyPrefab == prefab)
            {
                return enemyData;
            }
        }
        return null;
    }

    public EnemyController SpawnEnemy(Vector3 position)
    {
        EnemyController enemy = GetRandomEnemy();
        if (enemy != null)
        {
            enemy.transform.position = position;
        }
        return enemy;
    }

    public EnemyController SpawnEnemy(Vector3 position, GameObject prefab)
    {
        EnemyController enemy = GetEnemy(prefab);
        if (enemy != null)
        {
            enemy.transform.position = position;
        }
        return enemy;
    }

    public EnemyController SpawnEnemyNearPlayer(Transform playerTransform)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition(playerTransform);
        return SpawnEnemy(spawnPosition);
    }

    public EnemyController SpawnEnemyNearPlayer(Transform playerTransform, GameObject prefab)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition(playerTransform);
        return SpawnEnemy(spawnPosition, prefab);
    }

    private Vector3 GetRandomSpawnPosition(Transform playerTransform)
    {
        Vector3 spawnPosition;
        int attempts = 0;
        int maxAttempts = 10;

        do
        {
            // Generate random position around player
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(_minSpawnDistance, _maxSpawnDistance);
            spawnPosition = playerTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Check if position is valid (on NavMesh)
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
                break;
            }

            attempts++;
        } while (attempts < maxAttempts);

        // Fallback to player position + offset if no valid position found
        if (attempts >= maxAttempts)
        {
            spawnPosition = playerTransform.position + Vector3.forward * _minSpawnDistance;
        }

        return spawnPosition;
    }

    // Return All Methods
    public void ReturnAllEnemies()
    {
        foreach (var kvp in _enemyPools)
        {
            kvp.Value.ReturnAll();
        }
        
        foreach (var kvp in _activeEnemies)
        {
            kvp.Value.Clear();
        }
    }

    public void ReturnAllEnemies(GameObject prefab)
    {
        if (_enemyPools.ContainsKey(prefab))
        {
            _enemyPools[prefab].ReturnAll();
            _activeEnemies[prefab].Clear();
        }
    }

    // Statistics Methods
    public int GetActiveEnemyCount()
    {
        int total = 0;
        foreach (var kvp in _activeEnemies)
        {
            total += kvp.Value.Count;
        }
        return total;
    }

    public int GetActiveEnemyCount(GameObject prefab)
    {
        if (_activeEnemies.ContainsKey(prefab))
        {
            return _activeEnemies[prefab].Count;
        }
        return 0;
    }

    public int GetPooledEnemyCount()
    {
        int total = 0;
        foreach (var kvp in _enemyPools)
        {
            // This is an approximation since ObjectPool doesn't expose pool count
            total += _defaultPoolSize;
        }
        return total - GetActiveEnemyCount();
    }

    public int GetPooledEnemyCount(GameObject prefab)
    {
        if (_enemyPools.ContainsKey(prefab))
        {
            var enemyData = GetEnemyData(prefab);
            int poolSize = enemyData != null ? enemyData.poolSize : _defaultPoolSize;
            return poolSize - GetActiveEnemyCount(prefab);
        }
        return 0;
    }

    // Get available enemy types
    public GameObject[] GetAvailableEnemyTypes()
    {
        return _spawnablePrefabs.ToArray();
    }

    // Clear pool (for cleanup) - Kill all enemies before clearing
    public void ClearPool()
    {
        // Kill all enemies first
        KillAllEnemies();
        
        // Then return all to pool
        ReturnAllEnemies();
    }
    
    // Kill all active enemies
    public void KillAllEnemies()
    {
        foreach (var pool in _enemyPools.Values)
        {
            foreach (var enemy in pool.GetActiveObjects())
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy)
                {
                    var enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (enemyHealth != null && !enemyHealth.IsDead())
                    {
                        // Kill enemy by setting health to 0
                        enemyHealth.TakeDamage(enemyHealth.GetMaxHealth());
                        
                        Debug.Log($"Killed enemy: {enemy.name}");
                    }
                }
            }
        }
        
        Debug.Log("All enemies have been killed!");
    }
}
