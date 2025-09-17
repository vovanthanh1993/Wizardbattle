using Fusion;
using Fusion.Photon.Realtime;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkRunnerHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkRunnerHandler Instance { get; private set; }

    [SerializeField] private NetworkRunner _runnerPrefab;

    [SerializeField] private NetworkPrefabRef _lobbyManagerPrefab;

    private NetworkRunner _runner;
    private bool _sceneReady = false;
    private bool _isLeavingLobby = false; // Flag to track if player is leaving lobby voluntarily

    public bool IsRunning => _runner != null && _runner.IsRunning;
    public NetworkRunner Runner => _runner;
    
    /// <summary>
    /// Call this when player voluntarily leaves lobby to avoid showing disconnect popup
    /// </summary>
    public void SetLeavingLobby(bool isLeaving)
    {
        _isLeavingLobby = isLeaving;
    }

    private List<string> _existingRoomNames = new List<string>();
    private TaskCompletionSource<List<SessionInfo>> _sessionListTcs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Refresh the room list by joining the lobby.
    /// </summary>
    public async void RefreshRoomList()
    {
        await JoinLobbyAsync();
    }

    public async void JoinLobby()
    {
        await JoinLobbyAsync();
    }

    private void CreateRunner()
    {
        if (_runner == null)
        {
            _runner = Instantiate(_runnerPrefab);
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);
        }
    }

    private async Task JoinLobbyAsync()
    {
        CreateRunner();
        var result = await _runner.JoinSessionLobby(GameConstants.LOBBY);
        if (!result.Ok)
        {
            UIManager.Instance.ShowNoticePopup(GameConstants.STATUS_FAILED_LOBBY + result.ShutdownReason);
        }
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

        _existingRoomNames.Clear();
        foreach (var session in sessionList)
        {
            _existingRoomNames.Add(session.Name);
        }

        if (_sessionListTcs != null)
        {
            _sessionListTcs.TrySetResult(sessionList);
            _sessionListTcs = null;
        }

        UIManager.Instance.UpdateRoomListUI(sessionList);
    }


    public async void QuickJoinOrCreateRoom()
    {
        CreateRunner();

        UIManager.Instance.ShowLoadingPanel(true);
        await _runner.JoinSessionLobby(GameConstants.LOBBY);

        _sessionListTcs = new TaskCompletionSource<List<SessionInfo>>();

        List<SessionInfo> sessionList = await _sessionListTcs.Task;

        if (sessionList.Count > 0)
        {
            var randomRoom = sessionList[UnityEngine.Random.Range(0, sessionList.Count)];
            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = randomRoom.Name,
                Scene = SceneRef.FromIndex(GameConstants.LOBBY_SCENE_INDEX),
                SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            if (result.Ok)
            {
                UIManager.Instance.ShowLobby();
            }
            else
            {
                UIManager.Instance.ShowNoticePopup(GameConstants.STATUS_FAILED_JOIN_ROOM);
            }
        }
        else
        {
            // Create new room
            string roomName = UnityEngine.Random.Range(GameConstants.RANDOM_ROOM_MIN, GameConstants.RANDOM_ROOM_MAX).ToString();
            var result = await _runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Host,
                SessionName = roomName,
                Scene = SceneRef.FromIndex(GameConstants.LOBBY_SCENE_INDEX),
                SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
                IsVisible = true
            });

            if (result.Ok)
            {
                UIManager.Instance.ShowLobby();
                Runner.Spawn(_lobbyManagerPrefab, Vector3.zero, Quaternion.identity, null);
            }
            else
            {
                UIManager.Instance.ShowNoticePopup(GameConstants.STATUS_FAILED_CREATE_ROOM);
            }
        }
        UIManager.Instance.ShowLoadingPanel(false);
    }

    public async void ConnectToSession(string roomName, GameMode mode)
    {
        CreateRunner();
        UIManager.Instance.ShowLoadingPanel(true);
        await _runner.JoinSessionLobby(GameConstants.LOBBY);

        string finalRoomName = roomName;

        // Host Game
        if (mode == GameMode.Host)
        {
            finalRoomName = GenerateUniqueRoomName(roomName);
        }
        else if (mode == GameMode.Client && !_existingRoomNames.Contains(roomName))
        {
            UIManager.Instance.ShowNoticePopup(GameConstants.STATUS_ROOM_NOT_FOUND);
            UIManager.Instance.ShowLoadingPanel(false);
            return;
        }

        StartGameArgs args = new StartGameArgs
        {
            GameMode = mode,
            SessionName = finalRoomName,
            Scene = SceneRef.FromIndex(GameConstants.LOBBY_SCENE_INDEX),
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
            IsVisible = true,
        };

        var result = await _runner.StartGame(args);
        if (result.Ok)
        {
            if (mode == GameMode.Host) Runner.Spawn(_lobbyManagerPrefab, Vector3.zero, Quaternion.identity, null);
            UIManager.Instance.ShowLobby();
            UIManager.Instance.multiplayerManager.ShowJoinPanel(false);
        }
        else
        {
            UIManager.Instance.ShowLoadingPanel(false);
            UIManager.Instance.ShowNoticePopup($"{GameConstants.STATUS_FAILED_CONNECT}");
        }
    }

    private string GenerateUniqueRoomName(string roomName)
    {
        string finalRoomName = roomName;
        while (_existingRoomNames.Contains(finalRoomName))
        {
            int randomSuffix = UnityEngine.Random.Range(GameConstants.RANDOM_ROOM_SUFFIX_MIN, GameConstants.RANDOM_ROOM_SUFFIX_MAX);
            finalRoomName = $"{roomName}_{randomSuffix}";
        }
        return finalRoomName;
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            Debug.Log($"Local player {player} joined lobby");
            
            // Chờ LobbyManager được khởi tạo
            StartCoroutine(WaitForLobbyManagerAndAddPlayer(runner, player));
        }
        else
        {
            Debug.Log($"Remote player {player} joined lobby");
        }
    }

    private System.Collections.IEnumerator WaitForLobbyManagerAndAddPlayer(NetworkRunner runner, PlayerRef player)
    {
        // Chờ đến khi LobbyManager.Instance != null
        while (LobbyManager.Instance == null)
        {
            yield return null;
        }
        
        Debug.Log("LobbyManager.Instance is now available");
        
        // Lấy displayName từ Firebase
        var playerData = FirebaseDataManager.Instance?.GetCurrentPlayerData();
        string prefabName = playerData?.playerPrefabName ?? "Player_Mage";
        string displayName = playerData?.displayName ?? "Unknown Player";
        
        if (runner.IsServer)
        {
            // Host thêm trực tiếp
            LobbyManager.Instance.AddPlayerData(player, displayName, prefabName);
            Debug.Log($"Added player data (host): {displayName}");
        }
        else
        {
            // Client gửi lên host
            LobbyManager.Instance.RpcSendPlayerData(player, displayName, prefabName);
            Debug.Log($"Sent player data to host: {displayName}");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left lobby");
        
        // Host xóa player data
        if (Runner.IsServer)
        {
            LobbyManager.Instance.RemovePlayerData(player);
            Debug.Log($"Removed player data for player {player}");
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        _sceneReady = true;
        UIManager.Instance.ShowLoadingPanel(false);
    }

    /// <summary>
    /// Called when the NetworkRunner is shutting down
    /// Handles returning players to menu if they're in lobby when shutdown occurs
    /// </summary>
    /// <param name="runner">The NetworkRunner instance</param>
    /// <param name="shutdownReason">Reason for the shutdown</param>
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        // If in lobby when server shuts down, return to menu
        if (LobbyManager.Instance.GameState == GameState.Lobby)
        {
            Debug.Log("Server shutdown during lobby, returning to menu");
            
            // Only show popup if not voluntarily leaving lobby
            if (!_isLeavingLobby)
            {
                // Only show popup for clients, not for host
                if (!runner.IsServer)
                {
                    UIManager.Instance.ShowNoticePopup("Host left the lobby, returning to menu!");
                }
            }
            
            _isLeavingLobby = false;
            UIManager.Instance.BackToMenu();
        }
        else if (LobbyManager.Instance.GameState != GameState.Ended)
        {
            // Show disconnect popup for other game states
            UIManager.Instance.ShowDisconnectPopup(true);
        }
    }
    
    /// <summary>
    /// Called when disconnected from server (without specific reason)
    /// Handles returning players to menu if they're in lobby when disconnect occurs
    /// </summary>
    /// <param name="runner">The NetworkRunner instance</param>
    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        // If in lobby when disconnected, return to menu
        if (LobbyManager.Instance.GameState == GameState.Lobby)
        {
            Debug.Log("Disconnected from server during lobby. Return to menu!");
            UIManager.Instance.BackToMenu();
            UIManager.Instance.ShowNoticePopup("Disconnected from server during lobby. Return to menu!");
        }
        else if (LobbyManager.Instance.GameState != GameState.Ended)
        {
            // Show disconnect popup for other game states
            UIManager.Instance.ShowDisconnectPopup(true);
        }
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    /// <summary>
    /// Called when disconnected from server with specific reason
    /// Handles returning players to menu if they're in lobby when disconnect occurs
    /// </summary>
    /// <param name="runner">The NetworkRunner instance</param>
    /// <param name="reason">Specific reason for the disconnect</param>
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) 
    {
        // If in lobby when disconnected, return to menu
        if (LobbyManager.Instance.GameState == GameState.Lobby)
        {
            Debug.Log($"Disconnected from server during lobby (reason: {reason}), returning to menu");
            UIManager.Instance.BackToMenu();
        }
        else if (LobbyManager.Instance.GameState != GameState.Ended)
        {
            // Show disconnect popup for other game states
            UIManager.Instance.ShowDisconnectPopup(true);
        }
    }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
}