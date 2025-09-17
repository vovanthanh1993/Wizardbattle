using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class PlayerSpawnManager : MonoBehaviour
{
    public static PlayerSpawnManager Instance { get; private set; }

    [SerializeField] private Transform[] _spawnPoints;
    
    [Header("Prefab Mapping")]
    [SerializeField] private string[] _prefabNames;
    [SerializeField] private NetworkPrefabRef[] _prefabRefs;
    
    private Dictionary<string, NetworkPrefabRef> _prefabDictionary;

    private List<int> _availableSpawnIndices;

    private NetworkRunner Runner => NetworkRunnerHandler.Instance.Runner;

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
        _availableSpawnIndices = new List<int>();
        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            _availableSpawnIndices.Add(i);
        }
        
        // Tạo Dictionary từ 2 arrays
        _prefabDictionary = new Dictionary<string, NetworkPrefabRef>();
        for (int i = 0; i < _prefabNames.Length && i < _prefabRefs.Length; i++)
        {
            _prefabDictionary[_prefabNames[i]] = _prefabRefs[i];
            Debug.Log($"Mapped: {_prefabNames[i]} -> {_prefabRefs[i]}");
        }
    }

    public Transform GetSpawnPoint()
    {
        if (_availableSpawnIndices.Count == 0)
        {
            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                _availableSpawnIndices.Add(i);
            }
        }

        int randomIndex = Random.Range(0, _availableSpawnIndices.Count);
        int spawnIndex = _availableSpawnIndices[randomIndex];
        Transform spawnPoint = _spawnPoints[spawnIndex];

        _availableSpawnIndices.RemoveAt(randomIndex);

        return spawnPoint;
    }

    public void SpawnPlayer(PlayerRef player)
    {
        Transform spawnPoint = GetSpawnPoint();
        
        string selectedCharacterName = LobbyManager.Instance.GetPlayerPrefabName(player);
        Debug.Log("Selected character name: " + selectedCharacterName);
        NetworkPrefabRef selectedPrefab = GetPlayerPrefabByName(selectedCharacterName);
        Debug.Log("Selected prefab: " + selectedPrefab);
        var playerObject = Runner.Spawn(selectedPrefab, spawnPoint.position, spawnPoint.rotation, player);
        Runner.SetPlayerObject(player, playerObject);

        Debug.Log($"Spawned player {player} with character: {selectedCharacterName}");
    }

    private NetworkPrefabRef GetPlayerPrefabByName(string characterName)
    {
        return _prefabDictionary[characterName];
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcDisplayJoinMessage(string playerName)
    {
        if (UIManager.Instance != null)
        {
            //UIManager.Instance.SetStatus($"{playerName} has joined the game");
        }
    }
}
