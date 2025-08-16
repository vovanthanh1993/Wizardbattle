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
    [SerializeField] private GameObject _connectingPanel;
    [SerializeField] private GameObject _gameplayPanel;

    [Header("Status Text")]
    [SerializeField] private TMP_Text _statusText;

    [Header("Room UI")]
    [SerializeField] private TMP_InputField _roomInput;
    [SerializeField] private Button _createButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _quickJoinButton;
    [SerializeField] private Button _refreshButton;
    [SerializeField] private Button _backToMenuButton;
    [SerializeField] private Button _joinBackButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _resumeButton;

    [Header("Room List")]
    [SerializeField] private Transform _roomListParent;
    [SerializeField] private GameObject _roomEntryPrefab;

    [Header("Health UI")]
    [SerializeField] private Image _healthBarImage;

    [SerializeField] private Transform _leaderBoardContent;
    [SerializeField] private GameObject _linePrefab;
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private GameObject _createPanel;
    [SerializeField] private TMP_InputField _createRoomInput;
    [SerializeField] private Button _createOKButton;
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private GameObject _joinPanel;
    [SerializeField] private TMP_InputField _joinRoomInput;
    [SerializeField] private Button _joinOKButton;
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
    
    private List<RoomData> _currentRoomList = new List<RoomData>();

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
        InitPlayerName();
        ShowMenu();
        _createButton.onClick.AddListener(HandleCreateClicked);
        _joinButton.onClick.AddListener(HandleJoinClicked);
        _quickJoinButton.onClick.AddListener(HandleQuickJoinClicked);
        _refreshButton.onClick.AddListener(HandleRefreshRoomClicked);
        _backToMenuButton.onClick.AddListener(OnBackToMenuClicked);
        _createOKButton.onClick.AddListener(HandleCreateOKClicked);
        _joinOKButton.onClick.AddListener(HandleJoinOKClicked);
        _joinBackButton.onClick.AddListener(ShowMenu);
        _quitButton.onClick.AddListener(QuitGame);
        _resumeButton.onClick.AddListener(HandleResumeClicked);
        _roomScrollView.OnCellClicked(HandleCellClicked);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(GameConstants.LOBBY_SCENE);
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

    private void HandleCreateOKClicked()
    {
        if (!IsPlayerNameValid()) return;

        string roomName = _createRoomInput.text.Trim();

        if (string.IsNullOrEmpty(roomName))
        {
            SetStatus(GameConstants.ROOM_NAME_REQUIRED);
            return;
        }

        _createPanel.SetActive(false);
        ShowConnecting(GameConstants.CREATING_ROOM);
        NetworkRunnerHandler.Instance.ConnectToSession(roomName, GameMode.Host);
    }

    private void OnBackToMenuClicked()
    {
        SceneManager.LoadScene(GameConstants.LOBBY_SCENE);
        Destroy(NetworkRunnerHandler.Instance.Runner.gameObject);
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

    private void HandleCreateClicked()
    {
        if (!IsPlayerNameValid()) return;
        _createPanel.SetActive(true);
        _joinPanel.SetActive(false);
        _menuPanel.SetActive(false);
    }

    private void HandleJoinClicked()
    {
        if (!IsPlayerNameValid()) return;
        NetworkRunnerHandler.Instance.JoinLobby();
        ShowConnecting(GameConstants.JOINING_ROOM);
    }

    public void ShowJoinPanel()
    {
        _joinPanel.SetActive(true);
        _createPanel.SetActive(false);
        _menuPanel.SetActive(false);
    }

    private void HandleJoinOKClicked()
    {
        string roomName = string.IsNullOrEmpty(_joinRoomInput.text) ? GameConstants.DEFAULT_ROOM_NAME : _joinRoomInput.text;
        ShowConnecting(GameConstants.JOINING_ROOM);
        NetworkRunnerHandler.Instance.ConnectToSession(roomName, GameMode.Client);
    }

    private void HandleQuickJoinClicked()
    {
        if (!IsPlayerNameValid()) return;
        ShowConnecting(GameConstants.SEARCHING_ROOM);
        NetworkRunnerHandler.Instance.QuickJoinOrCreateRoom();
    }

    public void ShowMenu()
    {
        _menuPanel.SetActive(true);
        _connectingPanel.SetActive(false);
        _gameplayPanel.SetActive(false);
        _createPanel.SetActive(false);
        _joinPanel.SetActive(false);
        _inGameButtonsPanel.SetActive(false);
        SetStatus("");
    }

    public void ShowConnecting(string message = GameConstants.CONNECTING)
    {
        _createPanel.SetActive(false);
        _menuPanel.SetActive(false);
        _connectingPanel.SetActive(true);
        _gameplayPanel.SetActive(false);
        _joinPanel.SetActive(false);
        SetStatus(message);
    }

    public void ShowGameplay()
    {
        _createPanel.SetActive(false);
        _menuPanel.SetActive(false);
        _connectingPanel.SetActive(false);
        _gameplayPanel.SetActive(true);
        _joinPanel.SetActive(false);
        SetStatus(GameConstants.CONNECTED_STATUS);

        if (_roomNameText != null && NetworkRunnerHandler.Instance != null)
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
    }

    public void UpdateHealth(float current, float maxHealth)
    {
        float fill = Mathf.Clamp01(current / maxHealth);
        if (_healthBarImage != null)
            _healthBarImage.fillAmount = fill;
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
        _connectingPanel.SetActive(false);
        //_winnerText.text = string.Format(GameConstants.WIN_GAME_FORMAT, winnerName);
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
            SetStatus(GameConstants.PLAYER_NAME_REQUIRED);
            return false;
        }

        PlayerPrefs.SetString(GameConstants.PLAYER_PREFS_NAME_KEY, name);
        PlayerPrefs.Save();
        return true;
    }

    private void InitPlayerName()
    {
        if (PlayerPrefs.HasKey(GameConstants.PLAYER_PREFS_NAME_KEY))
            _playerNameInput.text = PlayerPrefs.GetString(GameConstants.PLAYER_PREFS_NAME_KEY);
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
        ShowConnecting(GameConstants.JOINING_ROOM);
        SetStatus(string.Format(GameConstants.JOIN_ROOM_FORMAT, roomNameToJoin));
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
}
