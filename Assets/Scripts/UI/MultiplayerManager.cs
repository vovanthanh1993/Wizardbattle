using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _quickJoinButton;

    [SerializeField] private TMP_InputField _createRoomInput;

    [SerializeField] private GameObject _createPanel;

    void Start() {
        _createRoomButton.onClick.AddListener(HandleCreateRoom);
        _joinButton.onClick.AddListener(HandleJoinClicked);
        _quickJoinButton.onClick.AddListener(HandleQuickJoin);
    }

    private void HandleJoinClicked()
    {
        NetworkRunnerHandler.Instance.JoinLobby();
        //ShowConnecting(GameConstants.JOINING_ROOM);
    }

    private void HandleQuickJoin()
    {
        //ShowConnecting(GameConstants.SEARCHING_ROOM);
        NetworkRunnerHandler.Instance.QuickJoinOrCreateRoom();
    }

    private void HandleCreateRoom()
    {
        string roomName = _createRoomInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            //SetStatus(GameConstants.ROOM_NAME_REQUIRED);
            return;
        }

        _createPanel.SetActive(false);
        //ShowConnecting(GameConstants.CREATING_ROOM);
        NetworkRunnerHandler.Instance.ConnectToSession(roomName, GameMode.Host);
    }

    /*private void HandleJoinOKClicked()
    {
        string roomName = string.IsNullOrEmpty(_joinRoomInput.text) ? GameConstants.DEFAULT_ROOM_NAME : _joinRoomInput.text;
        ShowConnecting(GameConstants.JOINING_ROOM);
        NetworkRunnerHandler.Instance.ConnectToSession(roomName, GameMode.Client);
    }*/
}
