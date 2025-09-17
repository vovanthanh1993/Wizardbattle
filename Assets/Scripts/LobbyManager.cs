using UnityEngine;
using Fusion;
using System.Linq;

public enum GameState
{
    Lobby,
    Waiting,
    Playing,
    Ended
}
public class LobbyManager : NetworkBehaviour
{
    
    public static LobbyManager Instance { get; private set; }

    [Networked, Capacity(20)]
    public NetworkDictionary<PlayerRef, string> PlayerNames => default;
    
    [Networked, Capacity(20)]
    public NetworkDictionary<PlayerRef, string> PrefabNames => default;

    [SerializeField] private int _minPlayersToStart = 2;
    [SerializeField] private int _maxPlayers = 8;

    [Networked] public GameState GameState { get; set; } = GameState.Lobby;

    public override void Spawned()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Runner.Despawn(Object);
        }

        Debug.Log($"[LobbyManager] Spawned on {(Object.HasStateAuthority ? "Host" : "Client")}");
    }

    

    /// <summary>
    /// Thêm player name vào danh sách (chỉ host mới có thể thực hiện)
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <param name="playerName">Tên player cần thêm</param>
    public void AddPlayerData(PlayerRef player, string playerName, string prefabName)
    {
        if (!Runner.IsServer) return;
        
        if (!PlayerNames.ContainsKey(player))
        {
            PlayerNames.Set(player, playerName);
            PrefabNames.Set(player, prefabName);
            RpcUpdateLobbyUI();
            RpcCheckIfReadyToStart();
            Debug.Log($"Added player name: {playerName} for player {player}");
        }
        else
        {
            Debug.Log($"Player name already exists for player {player}: {playerName}");
        }
    }

    /// <summary>
    /// Xóa player name khỏi danh sách (chỉ host mới có thể thực hiện)
    /// </summary>
    /// <param name="player">Player reference</param>
    public void RemovePlayerData(PlayerRef player)
    {
        if (!Runner.IsServer) return;
        
        if (PlayerNames.ContainsKey(player))
        {
            PlayerNames.Remove(player);
            PrefabNames.Remove(player);
            RpcUpdateLobbyUI();
            RpcCheckIfReadyToStart();
            Debug.Log($"Removed player name for player {player}");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUpdateLobbyUI()
    {
        // Cập nhật UI để hiển thị danh sách players trong lobby
        UIManager.Instance.LobbyPanel.UpdateLobbyUI(Runner.ActivePlayers.Count(), _minPlayersToStart, _maxPlayers);
    }

    /// <summary>
    /// Lấy danh sách tất cả player names
    /// </summary>
    /// <returns>Array chứa tất cả player names</returns>
    public string[] GetAllPlayerNames()
    {
        var result = new string[PlayerNames.Count];
        int index = 0;
        foreach (var kvp in PlayerNames)
        {
            result[index++] = kvp.Value;
        }
        return result;
    }

    /// <summary>
    /// Lấy số lượng players hiện tại
    /// </summary>
    /// <returns>Số lượng players</returns>
    public int GetPlayerCount()
    {
        return PlayerNames.Count;
    }

    /// <summary>
    /// Kiểm tra xem player có trong danh sách không
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <returns>True nếu player có trong danh sách</returns>
    public bool HasPlayer(PlayerRef player)
    {
        return PlayerNames.ContainsKey(player);
    }

    /// <summary>
    /// Lấy player name của một player cụ thể
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <returns>Player name hoặc null</returns>
    public string GetPlayerName(PlayerRef player)
    {
        if (PlayerNames.TryGet(player, out string playerName))
        {
            return playerName;
        }
        return null;
    }

    /// <summary>
    /// Lấy prefab name của một player cụ thể
    /// </summary>
    /// <param name="player">Player reference</param>
    /// <returns>Prefab name hoặc null</returns>
    public string GetPlayerPrefabName(PlayerRef player)
    {
        if (PrefabNames.TryGet(player, out string prefabName))
        {
            return prefabName;
        }
        return null;
    }

    /// <summary>
    /// RPC để client gửi player name lên host
    /// </summary>
    /// <param name="playerName">Tên player từ client</param>
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSendPlayerData(PlayerRef player, string playerName, string prefabName)
    {
        if (Runner.IsServer)
        {
            AddPlayerData(player, playerName, prefabName);
            Debug.Log($"Received player name from client: {playerName}");
        }
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
}
