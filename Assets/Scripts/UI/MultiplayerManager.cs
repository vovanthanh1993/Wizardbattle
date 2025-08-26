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
    [SerializeField] private TMP_InputField _joinRoomInput;
    [SerializeField] private Button _joinRoomButton;

    [SerializeField] private GameObject _createPanel;
    [SerializeField] private GameObject _joinPanel;

    void Start() {
        _createRoomButton.onClick.AddListener(HandleCreateRoom);
        _joinButton.onClick.AddListener(HandleJoinClicked);
        _quickJoinButton.onClick.AddListener(HandleQuickJoin);
        _joinRoomButton.onClick.AddListener(HandleJoinOKClicked);
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
}
