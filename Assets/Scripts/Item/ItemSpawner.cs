using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    public static ItemSpawner Instance { get; private set; }

    [SerializeField] private NetworkPrefabRef _healthItemPrefabRef;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 20f;

    [SerializeField] private NetworkPrefabRef _xpPrefabRef;
    [SerializeField] private Transform[] _spawnPointsXp;
    [SerializeField] private float _respawnXpDelay = 4f;
    [SerializeField] private int _initialXpItemCount = 5;

    [Header("Random Spawn Areas")]
    [SerializeField] private bool _useRandomSpawnArea = true;
    [SerializeField] private SpawnArea[] _spawnAreas = new SpawnArea[]
    {
        new SpawnArea { min = new Vector3(-10f, 0f, -10f), max = new Vector3(10f, 0f, 10f) },
        new SpawnArea { min = new Vector3(20f, 0f, -10f), max = new Vector3(40f, 0f, 10f) },
        new SpawnArea { min = new Vector3(-10f, 0f, 20f), max = new Vector3(10f, 0f, 40f) }
    };
    [SerializeField] private float _minDistanceBetweenItems = 2f;

    [System.Serializable]
    public class SpawnArea
    {
        public Vector3 min;
        public Vector3 max;
        public bool enabled = true;
    }

    private NetworkObject _currentHealthItem;
    private List<NetworkObject> _currentXpItems = new List<NetworkObject>();
    private List<Vector3> _usedPositions = new List<Vector3>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            SpawnHealthItem();
            SpawnInitialXpItems();
        }
    }

    public void OnHealthItemPickedUp()
    {
        if (!Object.HasStateAuthority) return;

        if (_currentHealthItem != null)
        {
            Runner.Despawn(_currentHealthItem);
            _currentHealthItem = null;
        }

        StartCoroutine(RespawnAfterDelay());
    }

    public void OnXpItemPickedUp(XpItem pickedUpItem)
    {
        if (!Object.HasStateAuthority) return;

        // Find and remove the specific XP item that was picked up
        var networkObject = pickedUpItem.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            _currentXpItems.Remove(networkObject);
            
            // Remove the position from used positions list
            Vector3 itemPosition = networkObject.transform.position;
            _usedPositions.RemoveAll(pos => Vector3.Distance(pos, itemPosition) < 0.1f);
            
            Runner.Despawn(networkObject);
        }

        StartCoroutine(RespawnXpAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnHealthItem();
    }

    private void SpawnHealthItem()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0) return;

        int randomIndex = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[randomIndex];

        _currentHealthItem = Runner.Spawn(_healthItemPrefabRef, spawnPoint.position, spawnPoint.rotation);
    }

    private void SpawnInitialXpItems()
    {
        _usedPositions.Clear(); // Clear used positions for fresh start

        for (int i = 0; i < _initialXpItemCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            var xpItem = Runner.Spawn(_xpPrefabRef, spawnPosition, Quaternion.identity);
            _currentXpItems.Add(xpItem);
            _usedPositions.Add(spawnPosition);
        }
    }

    private void SpawnXpItem()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        var xpItem = Runner.Spawn(_xpPrefabRef, spawnPosition, Quaternion.identity);
        _currentXpItems.Add(xpItem);
        _usedPositions.Add(spawnPosition);
    }

    private IEnumerator RespawnXpAfterDelay()
    {
        yield return new WaitForSeconds(_respawnXpDelay);
        SpawnXpItem();
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (_useRandomSpawnArea)
        {
            return GetRandomPositionInArea();
        }
        else
        {
            // Fallback to spawn points if random area is disabled
            if (_spawnPointsXp != null && _spawnPointsXp.Length > 0)
            {
                int randomIndex = Random.Range(0, _spawnPointsXp.Length);
                return _spawnPointsXp[randomIndex].position;
            }
            return Vector3.zero;
        }
    }

    private Vector3 GetRandomPositionInArea()
    {
        // Get all enabled spawn areas
        List<SpawnArea> enabledAreas = new List<SpawnArea>();
        foreach (var area in _spawnAreas)
        {
            if (area.enabled)
            {
                enabledAreas.Add(area);
            }
        }

        if (enabledAreas.Count == 0)
        {
            Debug.LogError("No enabled spawn areas found!");
            return Vector3.zero;
        }

        int maxAttempts = 100; // Prevent infinite loop
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Randomly select one of the enabled areas
            SpawnArea selectedArea = enabledAreas[Random.Range(0, enabledAreas.Count)];
            
            Vector3 randomPosition = new Vector3(
                Random.Range(selectedArea.min.x, selectedArea.max.x),
                Random.Range(selectedArea.min.y, selectedArea.max.y),
                Random.Range(selectedArea.min.z, selectedArea.max.z)
            );

            // Check if position is far enough from other items
            bool isValidPosition = true;
            foreach (Vector3 usedPos in _usedPositions)
            {
                if (Vector3.Distance(randomPosition, usedPos) < _minDistanceBetweenItems)
                {
                    isValidPosition = false;
                    break;
                }
            }

            if (isValidPosition)
            {
                return randomPosition;
            }

            attempts++;
        }

        // If we can't find a valid position, return a random position from a random area
        SpawnArea fallbackArea = enabledAreas[Random.Range(0, enabledAreas.Count)];
        return new Vector3(
            Random.Range(fallbackArea.min.x, fallbackArea.max.x),
            Random.Range(fallbackArea.min.y, fallbackArea.max.y),
            Random.Range(fallbackArea.min.z, fallbackArea.max.z)
        );
    }
}
