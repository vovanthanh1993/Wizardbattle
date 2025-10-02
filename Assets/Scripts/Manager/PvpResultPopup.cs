using UnityEngine;
using System.Collections;

public class PvpResultPopup : MonoBehaviour
{
    public static PvpResultPopup Instance { get; private set; }

    [SerializeField] private GameObject _background;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;

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
        _winPanel.SetActive(false);
        _losePanel.SetActive(false);
    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowResultPopup(bool isWin) {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _background.SetActive(true);
        _winPanel.SetActive(isWin);
        _losePanel.SetActive(!isWin);
        
        // Update player data for both win and lose
        UpdatePlayerDataAfterGame(isWin);
    }

    public void ShowResultPopupForPlayer() {
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
                    if (localPlayerStatus.IsWin)
                    {
                        _winPanel.SetActive(true);
                        _losePanel.SetActive(false);
                        
                        // Update data when win
                        UpdatePlayerDataAfterGame(true);
                        return;
                    }
                }
            }
        }
        _winPanel.SetActive(false);
        _losePanel.SetActive(true);
        
        // Update data when lose
        UpdatePlayerDataAfterGame(false);
    }

    public void ReturnMenu(){
         _winPanel.SetActive(false);
        _losePanel.SetActive(false);
        _background.SetActive(false);
        UIManager.Instance.BackToMenu();
    }

    

    private async void UpdatePlayerDataAfterGame(bool isWin)
    {
        await UpdatePlayerAttributesAfterGame(isWin);
    }

    private async System.Threading.Tasks.Task UpdatePlayerAttributesAfterGame(bool isWin)
    {
        try
        {
            // Check if FirebaseDataManager is available
            if (FirebaseDataManager.Instance == null)
            {
                Debug.LogError("❌ FirebaseDataManager.Instance is null!");
                return;
            }

            Debug.Log("🔍 Loading player data from Firebase...");
            
            // Get current PlayerData
            var currentData = FirebaseDataManager.Instance.GetCurrentPlayerData();
            if (currentData != null)
            {
                Debug.Log("✅ Player data loaded successfully from Firebase");
                
                // Debug current data
                Debug.Log("=== CURRENT PLAYER DATA ===");
                Debug.Log($"Damage: {currentData.damage}");
                Debug.Log($"Speed: {currentData.speed}");
                Debug.Log($"Level: {currentData.level}");
                Debug.Log($"XP: {currentData.xp}");
                Debug.Log($"Gold: {currentData.gold}");
                Debug.Log($"Ruby: {currentData.ruby}");
                Debug.Log($"Life: {currentData.food}");
                Debug.Log("==========================");

                float newXP = currentData.xp;
                float newGold = currentData.gold;
                float newRuby = currentData.ruby;

                if (isWin)
                {
                    newXP += 200f;          // Increase XP
                    newGold += 100f;        // Increase gold
                    newRuby += 2f;      // Increase ruby
                }
                else
                {
                    newXP += 100f;           // Small XP increase
                    newGold += 20f;         // Small gold increase
                    newRuby += 0f;
                }
                
                // Check and update level
                int newLevel = GameCommonUtils.CalculateLevelFromXP(newXP);
                
                // Update currentData with new values before saving
                currentData.level = newLevel;
                currentData.xp = newXP;
                currentData.gold = newGold;
                currentData.ruby = newRuby;
                
                // Update PlayerData
                bool attributesUpdated = await FirebaseDataManager.Instance.SavePlayerData(currentData);
                
                if (attributesUpdated)
                {
                    string result = isWin ? "WIN" : "LOSE";
                    Debug.Log($"✅ Player data updated successfully after {result}!");
                }
                else
                {
                    Debug.LogError("❌ Failed to update player data!");
                }
            }
            else
            {
                Debug.LogError("❌ Failed to load current player data!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Exception in UpdatePlayerAttributesAfterGame: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
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

    public IEnumerator ShowPopupAfterDelay(float delay, bool isWin)
    {
        // Wait for specified delay
        yield return new WaitForSeconds(delay);
        
        // Show victory popup
        if (PvpResultPopup.Instance != null)
        {
            Debug.Log("Showing victory popup after delay...");
            PvpResultPopup.Instance.ShowResultPopup(isWin);
        }
    }

    public void ShowLosePopup(float delay)
    {
        StartCoroutine(ShowPopupAfterDelay(delay, false));
    }
}
