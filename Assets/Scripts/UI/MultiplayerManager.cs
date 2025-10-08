using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Linq;

public class MultiplayerManager : MonoBehaviour
{
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private TMP_InputField _createRoomInput;
    [SerializeField] private TMP_InputField _joinRoomInput;
    [SerializeField] private Button _joinRoomButton;

    [SerializeField] private GameObject _createPanel;
    [SerializeField] private GameObject _joinPanel;

    [SerializeField] private RoomScrollView _roomScrollView;
    private List<RoomData> _currentRoomList = new List<RoomData>();

    void Start() {
        _createRoomButton.onClick.AddListener(HandleCreateRoom);
        _joinButton.onClick.AddListener(HandleJoinClicked);
        _quickJoinButton.onClick.AddListener(HandleQuickJoin);
        _joinRoomButton.onClick.AddListener(HandleJoinOKClicked);
        _refreshButton.onClick.AddListener(HandleRefreshRoomClicked);
        _roomScrollView.OnCellClicked(HandleCellClicked);
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
            UIManager.Instance.ShowNoticePopup(GameConstants.ROOM_NAME_REQUIRED);
            return;
        }

        _createPanel.SetActive(false);
        NetworkRunnerHandler.Instance.ConnectToSession(roomName, GameMode.Host);
    }

    private void HandleJoinOKClicked()
    {
        string roomName = string.IsNullOrEmpty(_joinRoomInput.text) ? GameConstants.DEFAULT_ROOM_NAME : _joinRoomInput.text;
        NetworkRunnerHandler.Instance.ConnectToSession(roomName, GameMode.Client);
    }

    public void ShowJoinPanel(bool isShow) {
        _joinPanel.SetActive(isShow);
    }

    public void HandleCellClicked(int index)
    {
        RoomData selectedRoom = this._currentRoomList[index];
        string roomNameToJoin = selectedRoom.RoomName;
        NetworkRunnerHandler.Instance.ConnectToSession(roomNameToJoin, GameMode.Client);
    }

    public void UpdateRoomListUI(List<SessionInfo> sessions)
    {
        List<RoomData> roomDataList = sessions.Select(session => new RoomData(
            roomName: session.Name,
            playerCount: session.PlayerCount,
            maxPlayers: session.MaxPlayers
        )).ToList();

        _currentRoomList = roomDataList;
        _roomScrollView.UpdateData(roomDataList);
    }

    public void HandleRefreshRoomClicked()
    {
        NetworkRunnerHandler.Instance.JoinLobby();
        UpdateRoomListUI(new List<SessionInfo>());
    }
}
