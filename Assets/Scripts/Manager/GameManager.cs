using Fusion;
using Fusion.Addons.KCC;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Waiting,
    Playing,
    Ended
}

public class GameManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    public static GameManager Instance { get; private set; }

    [Header("Win Condition")]
    [SerializeField] private int _killsToWin = 5;
    public int KillsToWin => _killsToWin;

    [Header("Player Settings")]
    [SerializeField] private NetworkPrefabRef _playerPrefabRef;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

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
        Camera mainCamera = Camera.main;
        mainCamera.GetComponent<CameraController>().SetTarget(null);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PvpResultPopup.Instance.ShowResultPopupForPlayer();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcUpdateLeaderboard()
    {
        UIManager.Instance.UpdateAllLeaderboards();
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

            UIManager.Instance.SetStatus($"{playerName} has left the game");
            Runner.Despawn(Runner.GetPlayerObject(player));
        }

        UIManager.Instance.UpdateAllLeaderboards();
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (!Runner.IsServer)
            return;
        Transform spawnPoint = GetSpawnPoint();
        var playerObject = Runner.Spawn(_playerPrefabRef, spawnPoint.position, spawnPoint.rotation, player);
        Runner.SetPlayerObject(player, playerObject);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RpcDisplayJoinMessage(string playerName)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetStatus($"{playerName} has joined the game");
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcShowKillFeed(string killer, string victim)
    {
        UIManager.Instance?.ShowKillFeed(killer, victim);
    }
}