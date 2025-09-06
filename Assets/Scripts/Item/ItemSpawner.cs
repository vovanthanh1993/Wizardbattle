using Fusion;
using System.Collections;
using UnityEngine;

public class ItemSpawner : NetworkBehaviour
{
    public static ItemSpawner Instance { get; private set; }

    [SerializeField] private NetworkPrefabRef _healthItemPrefabRef;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 20f;

    [SerializeField] private NetworkPrefabRef _coinPrefabRef;
    [SerializeField] private Transform[] _spawnPointsCoin;
    [SerializeField] private float _respawnCoinDelay = 10f;

    private NetworkObject _currentHealthItem;
    private NetworkObject _currentXpItem;

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
            SpawnCoinItem();
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

    public void OnXpItemPickedUp()
    {
        if (!Object.HasStateAuthority) return;

        if (_currentXpItem != null)
        {
            Runner.Despawn(_currentXpItem);
            _currentXpItem = null;
        }

        StartCoroutine(RespawnCoinAfterDelay());
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

    private void SpawnCoinItem()
    {
        if (_spawnPointsCoin == null || _spawnPointsCoin.Length == 0) return;

        int randomIndex = Random.Range(0, _spawnPointsCoin.Length);
        Transform spawnPoint = _spawnPointsCoin[randomIndex];

        _currentXpItem = Runner.Spawn(_coinPrefabRef, spawnPoint.position, spawnPoint.rotation);
    }

    private IEnumerator RespawnCoinAfterDelay()
    {
        yield return new WaitForSeconds(_respawnCoinDelay);
        SpawnCoinItem();
    }
}
