using Fusion;
using Fusion.Addons.KCC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    [SerializeField] private int _killsToWin = 5;
    public int KillsToWin => _killsToWin;

    [Header("Player Settings")]
    [SerializeField] private NetworkPrefabRef[] _playerPrefabRefs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Lobby Settings")]
    [SerializeField] private int _minPlayersToStart = 1;
    [SerializeField] private int _maxPlayers = 8;

    private List<int> _availableSpawnIndices;
    private int _currentPrefabIndex = 0;

    private NetworkRunner Runner => NetworkRunnerHandler.Instance.Runner;

    [Networked] public GameState GameState { get; set; } = GameState.Lobby;

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

    private NetworkPrefabRef GetPlayerPrefabByName(string characterName)
    {
        // Tìm NetworkPrefabRef theo tên character
        for (int i = 0; i < _playerPrefabRefs.Length; i++)
        {
            if (_playerPrefabRefs[i].ToString().Contains(characterName))
            {
                return _playerPrefabRefs[i];
            }
        }
        
        // Fallback về prefab đầu tiên nếu không tìm thấy
        Debug.LogWarning($"Character '{characterName}' not found, using default prefab");
        return _playerPrefabRefs[0];
    }

    public void EndGame()
    {
        foreach (var player in Runner.ActivePlayers)
        {
            var playerObject = Runner.GetPlayerObject(player);
            if (playerObject == null) continue;

            var playerController = playerObject.GetComponent<PlayerController>();
            var playerStatus = playerObject.GetComponent<PlayerStatus>();
            
            if (playerController != null)
            {
                playerController.SetIdleAnimation();
                playerController.SetDisable(true);
            }
        }

        RpcShowResultForAllPlayers();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcShowResultForAllPlayers()
    {
        GameState = GameState.Ended;
        Camera mainCamera = Camera.main;
        mainCamera.GetComponent<CameraController>().SetTarget(null);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PvpResultPopup.Instance.ShowResultPopupForPlayer();

        if(NetworkRunnerHandler.Instance.Runner != null)
        {
            StartCoroutine(DestroyNetworkRunnerAfterFrame());
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUpdateLeaderboard()
    {
        UIManager.Instance.UpdateAllLeaderboards();
    }

    private IEnumerator DestroyNetworkRunnerAfterFrame()
    {
        // Wait for one frame
        yield return null;
        
        // Now destroy the NetworkRunner
        if(NetworkRunnerHandler.Instance.Runner != null)
        {
            Destroy(NetworkRunnerHandler.Instance.Runner.gameObject);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            string playerName = Runner.GetPlayerObject(player)?.GetComponent<PlayerController>()?.PlayerName.ToString();
            if (string.IsNullOrEmpty(playerName))
            {
                playerName = "Unknown Player";
            }

            //UIManager.Instance.SetStatus($"{playerName} has left the game");
            Runner.Despawn(Runner.GetPlayerObject(player));
            
            if (GameState == GameState.Lobby)
            {
                RpcUpdateLobbyUI();
                RpcCheckIfReadyToStart();
            }
        }

        UIManager.Instance.UpdateAllLeaderboards();
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (!Runner.IsServer)
            return;
            
        Debug.Log($"Player {player} joined");
        
        if (GameState == GameState.Lobby)
        {
            RpcUpdateLobbyUI();
            RpcCheckIfReadyToStart();
        }
        else if (GameState == GameState.Waiting)
        {
            // Spawn player khi đang chuyển sang gameplay
            //SpawnPlayer(player);
        }
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUpdateLobbyUI()
    {
        // Cập nhật UI để hiển thị danh sách players trong lobby
        UIManager.Instance.LobbyPanel.UpdateLobbyUI(Runner.ActivePlayers.Count(), _minPlayersToStart, _maxPlayers);
    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcStartGame()
    {
        GameState = GameState.Waiting;
        Debug.Log("Game starting...");
        
        // UI updates cho tất cả clients
        UIManager.Instance.ShowLoadingPanel(true);
        UIManager.Instance.ShowGameplay();
        
        // Chỉ Server mới load scene
        if (Runner.IsServer)
        {
            Runner.LoadScene(SceneRef.FromIndex(GameConstants.GAMEPLAY_SCENE_INDEX));
        }
    }
    
    private IEnumerator StartGameplayAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        GameState = GameState.Playing;
        Debug.Log("Game is now playing!");
    }
    
    private void SpawnPlayer(PlayerRef player)
    {
        Transform spawnPoint = GetSpawnPoint();
        string selectedCharacterName = "Player_Mage"; // Default

        if (HasInputAuthority)
        {
            selectedCharacterName = FirebaseDataManager.Instance.GetCurrentPlayerData()?.playerPrefabName ?? "Player_Mage";
        }
        
        NetworkPrefabRef selectedPrefab = GetPlayerPrefabByName(selectedCharacterName);
        
        var playerObject = Runner.Spawn(selectedPrefab, spawnPoint.position, spawnPoint.rotation, player);
        Runner.SetPlayerObject(player, playerObject);
        
        Debug.Log($"Spawned player {player} with character: {selectedCharacterName}");
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcCheckIfReadyToStart()
    {
        if(GameState != GameState.Lobby) return;
        
        int playerCount = Runner.ActivePlayers.Count();
        if (playerCount >= _minPlayersToStart && playerCount <= _maxPlayers)
        {
            // Chỉ hiển thị nút Start Game cho Host (player đầu tiên)
            if (Runner.IsServer && Runner.LocalPlayer == Runner.ActivePlayers.First())
            {
                Debug.Log("Show Start Game Button");
                UIManager.Instance.LobbyPanel.ShowStartGameButton(true);
            }
            else
            {
                Debug.Log("Hide Start Game Button");
                UIManager.Instance.LobbyPanel.ShowStartGameButton(false);
            }
        }
        else
        {
            UIManager.Instance.LobbyPanel.ShowStartGameButton(false);
        }
    }
    
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestStartGame()
    {
        if (!Runner.IsServer) return;
        
        // Chỉ cho phép Host/Server start game
        if (Runner.LocalPlayer != Runner.ActivePlayers.First())
        {
            Debug.Log("Only the host can start the game");
            return;
        }
        
        int playerCount = Runner.ActivePlayers.Count();
        if (playerCount >= _minPlayersToStart && playerCount <= _maxPlayers)
        {
            RpcStartGame();
        }
        else
        {
            Debug.Log($"Cannot start game. Players: {playerCount}, Required: {_minPlayersToStart}-{_maxPlayers}");
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcDisplayJoinMessage(string playerName)
    {
        if (UIManager.Instance != null)
        {
            //UIManager.Instance.SetStatus($"{playerName} has joined the game");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcShowKillFeed(string killer, string victim)
    {
        UIManager.Instance?.ShowKillFeed(killer, victim);
    }
}