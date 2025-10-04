using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private float initialSpawnInterval = 4f;
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float intervalDecreaseRate = 0.1f;
    [SerializeField] private float intervalDecreaseInterval = 10f; // Decrease every 10 seconds
    [SerializeField] private int maxEnemies = 200;
    [SerializeField] private bool useRandomEnemyTypes = true;
    
    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;
    [SerializeField] private float gameStartTime;
    [SerializeField] private float currentSpawnInterval;
    
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private bool useRandomSpawnPoints = true;
    
    [Header("Enemy Type Override")]
    [SerializeField] private GameObject specificEnemyPrefab; // If set, only spawn this type
    
    [Header("Debug")]
    [SerializeField] private bool autoSpawn = true;
    
    private float lastSpawnTime;
    private float lastIntervalDecreaseTime;
    
    private void Awake() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        // Initialize spawn points if empty
        if (spawnPoints.Count == 0)
        {
            // Try to find spawn points in scene
            GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");
            foreach (GameObject spawnPoint in spawnPointObjects)
            {
                spawnPoints.Add(spawnPoint.transform);
            }
            
            // If still no spawn points, create default ones
            if (spawnPoints.Count == 0)
            {
                CreateDefaultSpawnPoints();
            }
        }
        
        // Initialize difficulty scaling
        gameStartTime = Time.time;
        currentSpawnInterval = initialSpawnInterval;
        lastIntervalDecreaseTime = Time.time;
    }
    
    private void CreateDefaultSpawnPoints()
    {
        // Create 4 default spawn points around origin
        Vector3[] defaultPositions = {
            new Vector3(10, 0, 10),
            new Vector3(-10, 0, 10),
            new Vector3(10, 0, -10),
            new Vector3(-10, 0, -10)
        };
        
        foreach (Vector3 pos in defaultPositions)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{spawnPoints.Count}");
            spawnPoint.transform.position = pos;
            spawnPoint.tag = "SpawnPoint";
            spawnPoints.Add(spawnPoint.transform);
        }
    }
    
    private void Update()
    {
        if (!autoSpawn) return;
        if (spawnPoints.Count == 0) return;
        
        // Update difficulty scaling
        if (enableDifficultyScaling)
        {
            UpdateDifficultyScaling();
        }
        
        // Check if we can spawn more enemies
        if (EnemyPoolManager.Instance != null)
        {
            int activeEnemies = EnemyPoolManager.Instance.GetActiveEnemyCount();
            
            if (activeEnemies < maxEnemies && Time.time - lastSpawnTime >= currentSpawnInterval)
            {
                SpawnEnemy();
                lastSpawnTime = Time.time;
            }
        }
    }
    
    private void UpdateDifficultyScaling()
    {
        // Decrease spawn interval over time
        if (Time.time - lastIntervalDecreaseTime >= intervalDecreaseInterval)
        {
            currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - intervalDecreaseRate);
            lastIntervalDecreaseTime = Time.time;
            
            Debug.Log($"Spawn interval decreased to: {currentSpawnInterval:F2}s");
        }
    }
    
    public void SpawnEnemy()
    {
        if (EnemyPoolManager.Instance != null && spawnPoints.Count > 0)
        {
            Transform spawnPoint = GetRandomSpawnPoint();
            if (spawnPoint != null)
            {
                EnemyController enemy;
                
                if (specificEnemyPrefab != null)
                {
                    // Spawn specific enemy type
                    enemy = EnemyPoolManager.Instance.SpawnEnemy(spawnPoint.position, specificEnemyPrefab);
                }
                else if (useRandomEnemyTypes)
                {
                    // Spawn random enemy type
                    enemy = EnemyPoolManager.Instance.SpawnEnemy(spawnPoint.position);
                }
                else
                {
                    // Spawn first available enemy type
                    var availableTypes = EnemyPoolManager.Instance.GetAvailableEnemyTypes();
                    if (availableTypes.Length > 0)
                    {
                        enemy = EnemyPoolManager.Instance.SpawnEnemy(spawnPoint.position, availableTypes[0]);
                    }
                    else
                    {
                        enemy = EnemyPoolManager.Instance.SpawnEnemy(spawnPoint.position);
                    }
                }
                
                if (enemy != null)
                {
                    Debug.Log($"Spawned {enemy.name} at position: {enemy.transform.position}");
                }
            }
        }
    }
    
    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;
        
        if (useRandomSpawnPoints)
        {
            // Get random spawn point
            int randomIndex = Random.Range(0, spawnPoints.Count);
            return spawnPoints[randomIndex];
        }
        else
        {
            // Get first available spawn point (for sequential spawning)
            return spawnPoints[0];
        }
    }
    
    public void SpawnEnemyAtPosition(Vector3 position)
    {
        if (EnemyPoolManager.Instance != null)
        {
            EnemyController enemy = EnemyPoolManager.Instance.SpawnEnemy(position);
            if (enemy != null)
            {
                Debug.Log($"Spawned enemy at position: {position}");
            }
        }
    }
    
    public void ReturnAllEnemies()
    {
        if (EnemyPoolManager.Instance != null)
        {
            EnemyPoolManager.Instance.ReturnAllEnemies();
            Debug.Log("Returned all enemies to pool");
        }
    }
    
    // Spawn Point Management
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint != null && !spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Add(spawnPoint);
        }
    }
    
    public void RemoveSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint != null && spawnPoints.Contains(spawnPoint))
        {
            spawnPoints.Remove(spawnPoint);
        }
    }
    
    public void ClearSpawnPoints()
    {
        spawnPoints.Clear();
    }
    
    public int GetSpawnPointCount()
    {
        return spawnPoints.Count;
    }
    
    // Difficulty Scaling Methods
    public void ResetDifficulty()
    {
        currentSpawnInterval = initialSpawnInterval;
        lastIntervalDecreaseTime = Time.time;
        gameStartTime = Time.time;
        Debug.Log("Difficulty reset to initial values");
    }
    
    public void SetDifficultyScaling(bool enabled)
    {
        enableDifficultyScaling = enabled;
    }
    
    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
    }
    
    public float GetGameTime()
    {
        return Time.time - gameStartTime;
    }
    
    public void SetSpawnInterval(float newInterval)
    {
        currentSpawnInterval = Mathf.Max(minSpawnInterval, newInterval);
    }
    
    public void ForceDecreaseInterval()
    {
        currentSpawnInterval = Mathf.Max(minSpawnInterval, currentSpawnInterval - intervalDecreaseRate);
        lastIntervalDecreaseTime = Time.time;
        Debug.Log($"Spawn interval manually decreased to: {currentSpawnInterval:F2}s");
    }
    
    // Debug methods
    /*
    [ContextMenu("Spawn Enemy")]
    private void DebugSpawnEnemy()
    {
        SpawnEnemy();
    }
    
    [ContextMenu("Return All Enemies")]
    private void DebugReturnAllEnemies()
    {
        ReturnAllEnemies();
    }
    
    private void OnGUI()
    {
        if (EnemyPoolManager.Instance == null) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 350, 250));
        GUILayout.Label($"Active Enemies: {EnemyPoolManager.Instance.GetActiveEnemyCount()}");
        GUILayout.Label($"Pooled Enemies: {EnemyPoolManager.Instance.GetPooledEnemyCount()}");
        GUILayout.Label($"Spawn Points: {GetSpawnPointCount()}");
        GUILayout.Label($"Random Spawn: {useRandomSpawnPoints}");
        GUILayout.Label($"Random Enemy Types: {useRandomEnemyTypes}");
        GUILayout.Label($"Current Spawn Interval: {currentSpawnInterval:F2}s");
        GUILayout.Label($"Game Time: {GetGameTime():F1}s");
        GUILayout.Label($"Difficulty Scaling: {enableDifficultyScaling}");
        
        if (specificEnemyPrefab != null)
        {
            GUILayout.Label($"Specific Type: {specificEnemyPrefab.name}");
        }
        
        if (GUILayout.Button("Spawn Enemy"))
        {
            SpawnEnemy();
        }
        
        if (GUILayout.Button("Return All"))
        {
            ReturnAllEnemies();
        }
        
        if (GUILayout.Button("Toggle Random Spawn"))
        {
            useRandomSpawnPoints = !useRandomSpawnPoints;
        }
        
        if (GUILayout.Button("Toggle Random Enemy Types"))
        {
            useRandomEnemyTypes = !useRandomEnemyTypes;
        }
        
        if (GUILayout.Button("Toggle Difficulty Scaling"))
        {
            enableDifficultyScaling = !enableDifficultyScaling;
        }
        
        if (GUILayout.Button("Reset Difficulty"))
        {
            ResetDifficulty();
        }
        
        if (GUILayout.Button("Force Decrease Interval"))
        {
            ForceDecreaseInterval();
        }
        
        if (GUILayout.Button("Create Default Spawn Points"))
        {
            CreateDefaultSpawnPoints();
        }
        
        if (GUILayout.Button("Clear Specific Type"))
        {
            specificEnemyPrefab = null;
        }
        GUILayout.EndArea();
    }*/
}
