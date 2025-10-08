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

    [Header("Room List")]
    [SerializeField] private Transform _roomListParent;
    [SerializeField] private GameObject _roomEntryPrefab;

    [SerializeField] private GameObject _disconnectPopup;

    [SerializeField] private TopLeftPanel _topLeftPanel;
    [SerializeField] private TopRightPanel _topRightPanel;
    
    public TopRightPanel TopRightPanel => _topRightPanel;
    public TopLeftPanel TopLeftPanel => _topLeftPanel;

    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private NoticePopup _noticePopup;

    [SerializeField] private GameObject _settingPopup;
    public SettingPopup SettingPopup => _settingPopup?.GetComponent<SettingPopup>();

    [SerializeField] public MultiplayerManager multiplayerManager;
    
    [Header("Lobby UI")]
    [SerializeField] private GameObject _lobbyPanel;

    public LobbyPanel LobbyPanel => _lobbyPanel?.GetComponent<LobbyPanel>();

    [Header("Item UI")]
    [SerializeField] public ItemInfo ItemInfo;
    [SerializeField] public ItemInfo ItemToolTip;

    [SerializeField] public InventoryPanel InventoryPanel;

    [SerializeField] public PVERewardPanel RewardPanel;

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

    public void ShowRewardPanel(MissionReward missionReward) {
        RewardPanel.SetData(missionReward.xpReward, missionReward.goldReward, missionReward.rubyReward, missionReward.foodReward);
        RewardPanel.gameObject.SetActive(true);
        AudioManager.Instance.PlayOpenRewardSound();
    }

    public void ShowLoadingPanel(bool isShow)
    {
        _loadingPanel.SetActive(isShow);
    }

    public void ShowSettingPopup(bool isShow)
    {
        _settingPopup.SetActive(isShow);
    }

    public void ShowNoticePopup(string text){
        _noticePopup.ShowNoticePopup(text);
    }

    private void Start()
    {
        ShowMenu();
    }

    public void BackToMenu()
    {
        GameCommonUtils.LoadScene(GameConstants.HOME_SCENE);
        ShowMenu();
    }

    public void ShowScoreBoard(bool active)
    {
        GamePlayPanel.ShowScoreBoard(active);
        GamePlayPanel.UpdateAllScoreBoard();
    }

    public void ShowMenu()
    {
        _menuPanel.SetActive(true);
        _gameplayPanel.SetActive(false);
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

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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
