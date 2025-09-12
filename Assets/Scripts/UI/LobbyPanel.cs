using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class LobbyPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _lobbyPlayerCountText;
    [SerializeField] private TMP_Text _lobbyPlayerListText;
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
        if (_lobbyPlayerListText == null || NetworkRunnerHandler.Instance?.Runner == null)
            return;
            
        var runner = NetworkRunnerHandler.Instance.Runner;
        var players = new List<string>();
        
        foreach (PlayerRef player in runner.ActivePlayers)
        {
            var obj = runner.GetPlayerObject(player);
            if (obj == null) continue;
            
            var controller = obj.GetComponent<PlayerController>();
            if (controller != null)
            {
                players.Add(controller.PlayerName.ToString());
            }
        }
        
        _lobbyPlayerListText.text = "Players in lobby:\n" + string.Join("\n", players);
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
        if (GameManager.Instance != null && NetworkRunnerHandler.Instance?.Runner != null)
        {
            // Chỉ cho phép Host start game
            var runner = NetworkRunnerHandler.Instance.Runner;
            if (runner.IsServer && runner.LocalPlayer == runner.ActivePlayers.First())
            {
                GameManager.Instance.RpcRequestStartGame();
            }
            else
            {
                Debug.Log("Only the host can start the game");
            }
        }
    }
    
    public void HandleLeaveLobbyClicked()
    {
        if (NetworkRunnerHandler.Instance?.Runner != null)
        {
            NetworkRunnerHandler.Instance.Runner.Shutdown();
        }
        UIManager.Instance.BackToMenu();
    }
}
