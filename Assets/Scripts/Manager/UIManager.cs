using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _gameplayPanel;
    public GamePlayPanel GamePlayPanel => _gameplayPanel?.GetComponent<GamePlayPanel>();

    [Header("Room UI")]
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Button _backToMenuButton;
    [SerializeField] private Button _resumeButton;

    [Header("Room List")]
    [SerializeField] private Transform _roomListParent;
    [SerializeField] private GameObject _roomEntryPrefab;
    
    [SerializeField] private GameObject _inGameButtonsPanel;
    
    [SerializeField] private RoomScrollView _roomScrollView;

    [SerializeField] private GameObject _disconnectPopup;
    
    private List<RoomData> _currentRoomList = new List<RoomData>();

    [SerializeField] private TopLeftPanel _topLeftPanel;
    [SerializeField] private TopRightPanel _topRightPanel;
    
    public TopRightPanel TopRightPanel => _topRightPanel;
    public TopLeftPanel TopLeftPanel => _topLeftPanel;

    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private NoticePopup _noticePopup;

    [SerializeField] public MultiplayerManager multiplayerManager;
    
    [Header("Lobby UI")]
    [SerializeField] private GameObject _lobbyPanel;

    public LobbyPanel LobbyPanel => _lobbyPanel?.GetComponent<LobbyPanel>();

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

    public void ShowLoadingPanel(bool isShow)
    {
        _loadingPanel.SetActive(isShow);
    }

    public void ShowNoticePopup(string text){
        _noticePopup.ShowNoticePopup(text);
    }

    public void ShowInGameMenu(bool isShow)
    {
        _inGameButtonsPanel.SetActive(isShow);
    }

    private void Start()
    {
        ShowMenu();
        _refreshButton.onClick.AddListener(HandleRefreshRoomClicked);
        _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        _resumeButton.onClick.AddListener(HandleResumeClicked);
        _roomScrollView.OnCellClicked(HandleCellClicked);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(GameConstants.HOME_SCENE);
        ShowMenu();
    }

    public void HandleResumeClicked()
    {
        ShowInGameMenu(false);
        InputManager.Instance.IsVisibleMenuInGame = false;
        if (!InputManager.Instance.IsVisibleLeaderBoard)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void OnBackToMenuClicked()
    {
        GameCommonUtils.LoadScene(GameConstants.HOME_SCENE);
        ShowMenu();
    }

    public void ShowScoreBoard(bool active)
    {
        GamePlayPanel.ShowScoreBoard(active);
        GamePlayPanel.UpdateAllScoreBoard();
    }

    private void HandleRefreshRoomClicked()
    {
        UpdateRoomListUI(new List<SessionInfo>());
    }

    public void ShowMenu()
    {
        _menuPanel.SetActive(true);
        _gameplayPanel.SetActive(false);
        _inGameButtonsPanel.SetActive(false);
        _disconnectPopup.SetActive(false);
        _lobbyPanel.SetActive(false);
        _topLeftPanel.InitData();
        _topRightPanel.InitData();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (NetworkRunnerHandler.Instance.Runner != null)
        {
            Destroy(NetworkRunnerHandler.Instance.Runner.gameObject);
        }
    }
    
    public void ShowLobby()
    {
        _menuPanel.SetActive(false);
        _gameplayPanel.SetActive(false);
        _inGameButtonsPanel.SetActive(false);
        _disconnectPopup.SetActive(false);
        _lobbyPanel.SetActive(true);
    }

    public void ShowGameplay()
    {
        _disconnectPopup.SetActive(false);
        _menuPanel.SetActive(false);
        _gameplayPanel.SetActive(true);
        multiplayerManager.ShowJoinPanel(false);
        _lobbyPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateHealth(float current, float maxHealth)
    {
        GamePlayPanel.UpdateHealth(current, maxHealth);
    }

    public void ShowWinScreen(string winnerName)
    {
        _menuPanel.SetActive(false);
        PvpResultPopup.Instance.ShowResultPopup(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void HandleCellClicked(int index)
    {
        RoomData selectedRoom = this._currentRoomList[index];
        string roomNameToJoin = selectedRoom.RoomName;
        //SetStatus(string.Format(GameConstants.JOIN_ROOM_FORMAT, roomNameToJoin));
        NetworkRunnerHandler.Instance.ConnectToSession(roomNameToJoin, GameMode.Client);
    }

    public void UpdateRoomListUI(List<SessionInfo> sessions)
    {
        List<RoomData> roomDataList = sessions.Select(session => new RoomData(
            roomName: session.Name,
            playerCount: session.PlayerCount,
            maxPlayers: session.MaxPlayers
        )).ToList();

        this._currentRoomList = roomDataList;
        _roomScrollView.UpdateData(roomDataList);
    }

    public void ShowDisconnectPopup(bool isShow)
    {
        _disconnectPopup.SetActive(isShow);
    }

    public void UpdateLevelUI(long amount)
    {
        _gameplayPanel.GetComponent<GamePlayPanel>().UpdateLevelUI(amount);
    }
    //-------Cheat code -------//
    private void Update() {
        CheatCode();
    }
    private async void CheatCode() {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            bool success = await FirebaseDataManager.Instance.ResetToDefault();
            if (success)
            {
                Debug.Log("Cheat Code F1: Reset to default values successful!");
                ShowNoticePopup("Reset to default values successful!");
                _topLeftPanel.InitData();
                _topRightPanel.InitData();
            }
            else
            {
                Debug.Log("Cheat Code F1: Reset failed!");
                ShowNoticePopup("Reset failed!");
            }
        }
    }
    //-------Cheat code -------//
}
