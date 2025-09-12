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

    [SerializeField] private Transform _leaderBoardContent;
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private TMP_Text _playerNameInput;
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private GameObject _countdownPanel;
    [SerializeField] private GameObject _leaderBoardPanel;
    [SerializeField] private GameObject _inGameButtonsPanel;
    [SerializeField] private List<LeaderboardItem> _leaderboardItemList = new();
    [SerializeField] private RoomScrollView _roomScrollView;

    [Header("Kill Feed")]
    [SerializeField] private TMP_Text _killFeedText;
    [SerializeField] private GameObject _killFeedBackGround;

    
    [Header("Skill UI")]
    [SerializeField] private Image _fireBallCoolDown;
    [SerializeField] private TMP_Text _fireBallCoolDownText;
    [SerializeField] private Image _jumpCoolDown;
    [SerializeField] private TMP_Text _jumpCoolDownText;
    [SerializeField] private Image _healingCoolDown;
    [SerializeField] private TMP_Text _healingCoolDownText;

    [SerializeField] private Image _stealthCoolDown;
    [SerializeField] private TMP_Text _stealthCoolDownText;

    [SerializeField] private GameObject _disconnectPopup;
    
    private List<RoomData> _currentRoomList = new List<RoomData>();
    [SerializeField] private TMP_Text _targetText;

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

    public void ShowReSpawnTime(string respawnMess)
    {
        _countdownPanel.SetActive(!string.IsNullOrEmpty(respawnMess));
        _countdownText.text = respawnMess;
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
        ResetLevel();
        SceneManager.LoadScene(GameConstants.HOME_SCENE);
        ShowMenu();
    }

    public void ShowLeaderBoard(bool active)
    {
        _leaderBoardPanel.SetActive(active);
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
        //SetStatus(GameConstants.CONNECTED_STATUS);

        /*if (_roomNameText != null && NetworkRunnerHandler.Instance != null)
        {
            string roomName = NetworkRunnerHandler.Instance.Runner?.SessionInfo?.Name;
            _roomNameText.text = string.IsNullOrEmpty(roomName)
                ? GameConstants.UNKNOWN_ROOM
                : string.Format(GameConstants.ROOM_NAME_DISPLAY_FORMAT, roomName);
        }
    }

    public void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.gameObject.SetActive(true);
            _statusText.text = message;
            StartCoroutine(HideStatusAfterDelay(2f));
        }
    }

    private IEnumerator HideStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _statusText.gameObject.SetActive(false);
    }*/
    }

    public void UpdateHealth(float current, float maxHealth)
    {
        GamePlayPanel.UpdateHealth(current, maxHealth);
    }

    public void UpdateAllLeaderboards()
    {
        var runner = NetworkRunnerHandler.Instance.Runner;
        if (runner == null) return;

        List<PlayerController> players = new();

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            var obj = runner.GetPlayerObject(player);
            if (obj == null) continue;

            var controller = obj.GetComponent<PlayerController>();
            if (controller == null) continue;

            players.Add(controller);
        }

        players = players.OrderByDescending(p => p.Kills).ToList();

        for (int i = 0; i < _leaderboardItemList.Count; i++)
        {
            if (i < players.Count)
            {
                var controller = players[i];
                _leaderboardItemList[i].SetData(i + 1, controller.PlayerName.ToString(), controller.Kills, controller.Deaths);
                _leaderboardItemList[i].gameObject.SetActive(true);
            }
            else
            {
                _leaderboardItemList[i].gameObject.SetActive(false);
            }
        }
    }

    public void ShowWinScreen(string winnerName)
    {
        _menuPanel.SetActive(false);
        PvpResultPopup.Instance.ShowResultPopup(true);
    }

    public void AddPlayerToLeaderboard(string playerName, int kills, int deaths)
    {
        GameObject line = Instantiate(_linePrefab, _leaderBoardContent);
        TextMeshProUGUI[] texts = line.GetComponentsInChildren<TextMeshProUGUI>();

        foreach (var text in texts)
        {
            switch (text.name)
            {
                case GameConstants.ORDER_TEXT_NAME:
                    text.text = playerName;
                    break;
                case GameConstants.NAME_TEXT_NAME:
                    text.text = playerName;
                    break;
                case GameConstants.KILL_TEXT_NAME:
                    text.text = kills.ToString();
                    break;
                case GameConstants.DEATH_TEXT_NAME:
                    text.text = deaths.ToString();
                    break;
            }
        }
    }

    public bool IsPlayerNameValid()
    {
        if (_playerNameInput == null) return false;

        string name = _playerNameInput.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            _noticePopup.ShowNoticePopup(GameConstants.PLAYER_NAME_REQUIRED);
            return false;
        }

        PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_NAME_KEY, name);
        PlayerPrefs.Save();
        return true;
    }

    public string GetPlayerName()
    {
        string name = _playerNameInput.text.Trim();
        return string.IsNullOrEmpty(name)
            ? GameConstants.DEFAULT_PLAYER_NAME_PREFIX + Random.Range(1000, 9999)
            : name;
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

    public void ShowKillFeed(string killer, string victim)
    {
        _killFeedText.text = $"<color=#00FF00>{killer}</color> killed <color=#FF0000>{victim}</color>";
        _killFeedBackGround.SetActive(true);
        StopCoroutine(nameof(HideKillFeedAfterDelay));
        StartCoroutine(HideKillFeedAfterDelay(2f));
    }

    private IEnumerator HideKillFeedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _killFeedBackGround.SetActive(false);
    }

    public void StartFireballCooldown(float duration)
    {
        StartCooldownRoutine("FireballCooldownRoutine", _fireBallCoolDown, _fireBallCoolDownText, duration);
    }

    public void StartJumpCooldown(float duration)
    {
        StartCooldownRoutine("JumpCooldownRoutine", _jumpCoolDown, _jumpCoolDownText, duration);
    }

    public void StartHealingCooldown(float duration)
    {
        StartCooldownRoutine("HealingCooldownRoutine", _healingCoolDown, _healingCoolDownText, duration);
    }

    public void StartStealthCooldown(float duration)
    {
        StartCooldownRoutine("StealthCooldownRoutine", _stealthCoolDown, _stealthCoolDownText, duration);
    }

    private void StartCooldownRoutine(string routineName, Image cooldownImage, TMP_Text cooldownText, float duration)
    {
        StopCoroutine(routineName);
        StartCoroutine(CooldownRoutine(cooldownImage, cooldownText, duration));
    }

    private IEnumerator CooldownRoutine(Image cooldownImage, TMP_Text cooldownText, float duration)
    {
        if (cooldownImage == null) yield break;

        // Setup cooldown UI
        cooldownImage.fillAmount = 1f;
        cooldownImage.gameObject.SetActive(true);
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
        }

        // Run cooldown timer
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cooldownImage.fillAmount = 1f - (elapsed / duration);
            
            if (cooldownText != null)
            {
                float remaining = Mathf.Max(0f, duration - elapsed);
                int seconds = Mathf.CeilToInt(remaining);
                cooldownText.text = seconds.ToString();
            }
            yield return null;
        }

        // Hide cooldown UI
        cooldownImage.fillAmount = 0f;
        cooldownImage.gameObject.SetActive(false);
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(false);
        }
    }

    public void ShowDisconnectPopup(bool isShow)
    {
        _disconnectPopup.SetActive(isShow);
    }

    public void UpdateLevelUI(long amount)
    {
        _gameplayPanel.GetComponent<GamePlayPanel>().UpdateLevelUI(amount);
    }

    public void UpdateTargetText(int num)
    {
        _targetText.text = num.ToString();
    }

    public void ResetLevel() {
        GamePlayPanel.ResetLevel();
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
