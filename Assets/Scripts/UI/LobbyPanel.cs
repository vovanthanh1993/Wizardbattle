using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyPlayerCountText;

    [SerializeField] private GameObject _content;
    [SerializeField] private GameObject _contentPrefab;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _leaveLobbyButton;
    public void UpdateLobbyUI(int currentPlayers, int minPlayers, int maxPlayers)
    {
        if (_lobbyPlayerCountText != null)
        {
            _lobbyPlayerCountText.text = $"Players: {currentPlayers}/{maxPlayers}"; 
            //(Min: {minPlayers})";
        }
        
        // Update player list
        UpdateLobbyPlayerList();
    }

    private void Start() {
        _startGameButton.onClick.AddListener(HandleStartGameClicked);
        _leaveLobbyButton.onClick.AddListener(HandleLeaveLobbyClicked);
    }
    
    private void UpdateLobbyPlayerList()
    {
        if (LobbyManager.Instance != null)
        {
            var players = LobbyManager.Instance.GetAllPlayerNames();
            
            // Xóa tất cả children hiện tại
            foreach (Transform child in _content.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Tạo UI cho mỗi player
            foreach (var player in players)
            {
                var obj = Instantiate(_contentPrefab, _content.transform);
                obj.GetComponent<PlayerLobbyInfo>().SetData(player);
            }
        }
    }
    
    public void ShowStartGameButton(bool show)
    {
        if (_startGameButton != null)
        {
            _startGameButton.gameObject.SetActive(show);
        }
    }
    
    public void HandleStartGameClicked()
    {
        // Chỉ cho phép Host start game
        var runner = NetworkRunnerHandler.Instance.Runner;
        if (runner.IsServer && runner.LocalPlayer == runner.ActivePlayers.First())
        {
            LobbyManager.Instance.RpcRequestStartGame();
        }
        else
        {
            Debug.Log("Only the host can start the game");
        }
    }
    
    public void HandleLeaveLobbyClicked()
    {
        if (NetworkRunnerHandler.Instance?.Runner != null)
        {
            // Set flag to indicate player is voluntarily leaving lobby
            NetworkRunnerHandler.Instance.SetLeavingLobby(true);
            NetworkRunnerHandler.Instance.Runner.Shutdown();
        }
    }
}
