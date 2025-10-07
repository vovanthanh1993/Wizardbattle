using UnityEngine;
using System.Collections;

public class GameResultPopup : MonoBehaviour
{
    public static GameResultPopup Instance { get; private set; }

    [SerializeField] private GameObject _background;
    [SerializeField] private PVERewardPanel _pveWinPanel;
    [SerializeField] private PVERewardPanel _pveLosePanel;
    [SerializeField] private PVPRewardPanel _pvpRewardPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _background.gameObject.SetActive(false);
        _pveWinPanel.gameObject.SetActive(false);
        _pveLosePanel.gameObject.SetActive(false);
        _pvpRewardPanel.gameObject.SetActive(false);
    }

    public void ShowPVPResult() {
        _pvpRewardPanel.gameObject.SetActive(true);
        _background.SetActive(true);
        var runner = NetworkRunnerHandler.Instance?.Runner;
        if (runner != null)
        {
            var localPlayer = runner.GetPlayerObject(runner.LocalPlayer);
            if (localPlayer != null)
            {
                var localPlayerStatus = localPlayer.GetComponent<PlayerStatus>();
                if (localPlayerStatus != null)
                {       
                    int rank = localPlayerStatus.Rank;  
                    int kills = localPlayerStatus.Kills;           
                    int xpReward = (int)(FirebaseDataManager.Instance.GetCurrentGameData().pvpXpReward / rank + kills*2);
                    int goldReward = (int)(FirebaseDataManager.Instance.GetCurrentGameData().pvpGoldReward / rank + kills);
                    int rubyReward = (int)(FirebaseDataManager.Instance.GetCurrentGameData().pvpRubyReward / rank + kills/2);
                    _pvpRewardPanel.SetData(rank, xpReward, goldReward, rubyReward);
                    UpdatePlayerDataAfterGame(xpReward, goldReward, rubyReward);
                }
            }
        }
    }

    public void ReturnMenu() {
        _pveWinPanel.gameObject.SetActive(false);
        _pveLosePanel.gameObject.SetActive(false);
        _pvpRewardPanel.gameObject.SetActive(false);
        _background.SetActive(false);
        UIManager.Instance.BackToMenu();
    }

    private async void UpdatePlayerDataAfterGame(int xpReward, int goldReward, int rubyReward)
    {
        await FirebaseDataManager.Instance.UpdatePlayerAttributesAfterGame(xpReward, goldReward, rubyReward);
    }

    

    private int GetPlayerKills()
    {
        var runner = NetworkRunnerHandler.Instance?.Runner;
        if (runner != null)
        {
            var localPlayer = runner.GetPlayerObject(runner.LocalPlayer);
            if (localPlayer != null)
            {
                var playerStatus = localPlayer.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    return playerStatus.Kills;
                }
            }
        }
        return 0;
    }

    private int GetPlayerDeaths()
    {
        var runner = NetworkRunnerHandler.Instance?.Runner;
        if (runner != null)
        {
            var localPlayer = runner.GetPlayerObject(runner.LocalPlayer);
            if (localPlayer != null)
            {
                var playerStatus = localPlayer.GetComponent<PlayerStatus>();
                if (playerStatus != null)
                {
                    return playerStatus.Deaths;
                }
            }
        }
        return 0;
    }

    public void ShowVictoryPopup(float delay)
    {
        StartCoroutine(ShowPopupAfterDelay(delay, true));
    }

    public void ShowLosePopup(float delay)
    {
        StartCoroutine(ShowPopupAfterDelay(delay, false));
    }

    public IEnumerator ShowPopupAfterDelay(float delay, bool isWin)
    {
        // Wait for specified delay
        yield return new WaitForSeconds(delay);
        if(isWin)
        {
            AudioManager.Instance.PlayPVEWinSound();
        }
        else
        {
            AudioManager.Instance.PlayPVELoseSound();
        }
        // Show victory popup
        ShowPVEResult(isWin);
    }

    public void ShowPVEResult(bool isWin) {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _background.SetActive(true);
        // Update player data for both win and lose
        int xpReward = (int)(FirebaseDataManager.Instance.GetCurrentGameData().pveXpReward);
        int goldReward = (int)(FirebaseDataManager.Instance.GetCurrentGameData().pveGoldReward);
        int rubyReward = (int)(FirebaseDataManager.Instance.GetCurrentGameData().pveRubyReward);

        if (isWin) {
            _pveWinPanel.gameObject.SetActive(true);
            _pveWinPanel.SetData(xpReward, goldReward, rubyReward);
            _pveLosePanel.gameObject.SetActive(false);
        } else {
            xpReward = xpReward/2;
            goldReward = goldReward/2;
            rubyReward = rubyReward/2;
            _pveWinPanel.gameObject.SetActive(false);
            _pveLosePanel.gameObject.SetActive(true);
            _pveLosePanel.SetData(xpReward, goldReward, rubyReward);
        }

        UpdatePlayerDataAfterGame(xpReward, goldReward, rubyReward);
    }
}
