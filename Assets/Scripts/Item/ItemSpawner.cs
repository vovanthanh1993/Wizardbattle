using Fusion;
using System.Collections;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    public static ItemSpawner Instance { get; private set; }

    [SerializeField] private NetworkPrefabRef _healthItemPrefabRef;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 20f;

    private NetworkObject _currentHealthItem;

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
        var healthItem = _currentHealthItem.GetComponent<HealthItem>();
    }
}
