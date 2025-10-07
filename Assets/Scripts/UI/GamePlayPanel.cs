using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using DG.Tweening;
public class GamePlayPanel : MonoBehaviour
{
    [SerializeField] private Image _xpBarImage;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private GameObject _xpBarObject;

    [Header("Health UI")]
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private TMP_Text _healthText;

    [Header("Level Calculation")]
    [SerializeField] private int _baseXP = 100;
    [SerializeField] private float _xpMultiplier = 1.5f;

    [Header("Skill UI")]
    [SerializeField] private GameObject _skillUI1;
    [SerializeField] private GameObject _skillUI2;
    [SerializeField] private GameObject _skillUI3;
    public bool IsEnableSkill1 { get; private set; }
    public bool IsEnableSkill2 { get; private set; }
    public bool IsEnableSkill3 { get; private set; }

    [Header("Boss Health UI")]
    [SerializeField] private Image _bossHealthBarImage;
    [SerializeField] private TMP_Text _bossHealthText;
    [SerializeField] private GameObject _bossHealthBar;

    [Header("Game Info UI")]
    [SerializeField] private GameObject _gameInfoPVPPanel;
    [SerializeField] private GameObject _gameInfoPVEPanel;
    [SerializeField] private TMP_Text _timeText;
    [SerializeField] private TMP_Text _countdownText;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private TMP_Text _killText;
    [SerializeField] private TMP_Text _deathText;
    [SerializeField] private float _statusTextDuration = 3f;
    [SerializeField] private float _statusTextFadeSpeed = 2f;

    [Header("ScoreBoard UI")]
    [SerializeField] private List<ScoreBoardItem> _scoreBoardItemList = new();
    [SerializeField] private GameObject _scoreBoardPanel;
    [SerializeField] private Transform _scoreBoardContent;
    [SerializeField] private GameObject _scoreBoardLinePrefab;

    [Header("Skill UI")]
    [SerializeField] private Image _fireBallCoolDown;
    [SerializeField] private TMP_Text _fireBallCoolDownText;
    [SerializeField] private Image _healingCoolDown;
    [SerializeField] private TMP_Text _healingCoolDownText;
    [SerializeField] private Image _runCoolDown;
    [SerializeField] private TMP_Text _runCoolDownText;

    [SerializeField] private Image _stealthCoolDown;
    [SerializeField] private TMP_Text _stealthCoolDownText;
    
    [Header("Kill Feed")]
    [SerializeField] private TMP_Text _killFeedText;
    [SerializeField] private GameObject _killFeedBackGround;

    [Header("Respawn Countdown")]

    [SerializeField] private TMP_Text _respawnCountdownText;
    [SerializeField] private GameObject _respawnCountdownPanel;

    [Header("In Game Buttons Panel")]
    [SerializeField] private GameObject _inGameButtonsPanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _menuButton;

    private void OnEnable() {
        ShowBossHealthBar(false);
        ShowScoreBoard(false);
        ResetLevel();
        InitUI();
        UpdateLevelUI(0);
        UpdateXpBar();
        UpdateKillText(0);
        UpdateDeathText(0);
        _resumeButton.onClick.AddListener(HandleResumeClicked);
        _settingsButton.onClick.AddListener(HandleSettingsClicked);
        _menuButton.onClick.AddListener(HandleMenuClicked);
    }

    public void ShowInGameMenu(bool isShow)
    {
        _inGameButtonsPanel.SetActive(isShow);
        Cursor.lockState = isShow ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isShow;
    }

     public void HandleResumeClicked()
    {
        ShowInGameMenu(false);
        InputManager.Instance.IsVisibleMenuInGame = false;
    }

    public void HandleMenuClicked()
    {
        GameCommonUtils.LoadScene(GameConstants.HOME_SCENE);
        UIManager.Instance.ShowMenu();
    }

    public void HandleSettingsClicked()
    {
        UIManager.Instance.ShowSettingPopup(true);
    }

    public void UpdateXpBar()
    {
       if (NetworkRunnerHandler.Instance.GameType == GameType.PVP) {
        _xpBarObject.SetActive(false);
       } else {
        _xpBarObject.SetActive(true);
       }
    }
    public void InitUI()
    {
        _runCoolDown.gameObject.SetActive(false);
        _healingCoolDown.gameObject.SetActive(false);
        _stealthCoolDown.gameObject.SetActive(false);
        _fireBallCoolDown.gameObject.SetActive(false);
        _fireBallCoolDownText.gameObject.SetActive(false);
        _healingCoolDownText.gameObject.SetActive(false);
        _runCoolDownText.gameObject.SetActive(false);
        _stealthCoolDownText.gameObject.SetActive(false);
        _killFeedBackGround.SetActive(false);
        _respawnCountdownPanel.SetActive(false);
        _respawnCountdownText.text = "";
        _statusText.text = "";
        _inGameButtonsPanel.SetActive(false);
        if (NetworkRunnerHandler.Instance.GameType == GameType.PVE) {
            _gameInfoPVPPanel.SetActive(false);
            _gameInfoPVEPanel.SetActive(true);
        } else {
            _gameInfoPVPPanel.SetActive(true);
            _gameInfoPVEPanel.SetActive(false);
        }
    }
    public void UpdateXpBar(long xp, long xpToNextLevel)
    {
        if (_xpBarImage != null)
        {
            float progress = (float)xp / xpToNextLevel;
            _xpBarImage.fillAmount = Mathf.Clamp01(progress);
        }
    }

    public void UpdateLevel(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"Level {level}";
        }
    }
    
    public void UpdateLevelUI(long xp)
    {
        int level = CalculateLevelFromXP(xp);
        IsEnableSkill1 = level >= 2 || NetworkRunnerHandler.Instance.GameType == GameType.PVP;
        IsEnableSkill2 = level >= 5 || NetworkRunnerHandler.Instance.GameType == GameType.PVP;
        IsEnableSkill3 = level >= 8 || NetworkRunnerHandler.Instance.GameType == GameType.PVP;
        _skillUI1.SetActive(!IsEnableSkill1);
        _skillUI2.SetActive(!IsEnableSkill2);
        _skillUI3.SetActive(!IsEnableSkill3);
        long currentLevelXP = CalculateCurrentLevelXP(xp, level);
        long xpToNextLevel = CalculateXPToNextLevel(level);
        
        UpdateLevel(level);
        UpdateXpBar(currentLevelXP, xpToNextLevel);
    }
    
    private int CalculateLevelFromXP(long xp)
    {
        if (xp <= 0) return 1;
        
        int level = 1;
        long totalXPNeeded = 0;
        
        while (totalXPNeeded <= xp)
        {
            level++;
            totalXPNeeded += CalculateXPForLevel(level);
        }
        return level - 1;
    }
    
    private long CalculateXPForLevel(int level)
    {
        if (level <= 1) return 0;
        return (long)(_baseXP * Mathf.Pow(_xpMultiplier, level - 2));
    }
    
    private long CalculateCurrentLevelXP(long totalXP, int currentLevel)
    {
        if (currentLevel <= 1) return totalXP;
        
        long xpForCurrentLevel = 0;
        for (int i = 2; i <= currentLevel; i++)
        {
            xpForCurrentLevel += CalculateXPForLevel(i);
        }
        
        return totalXP - xpForCurrentLevel;
    }
    
    private long CalculateXPToNextLevel(int currentLevel)
    {
        return CalculateXPForLevel(currentLevel + 1);
    }

    public void UpdateHealth(float current, float maxHealth)
    {
        float fillAmount = Mathf.Clamp01(current / maxHealth);
        if (_healthBarImage != null) {
            _healthBarImage.DOKill();
            _healthBarImage.DOFillAmount(fillAmount, 0.5f).SetEase(Ease.OutQuad);
            _healthText.text = $"{current}/{maxHealth}";
        }    
    }

    public void UpdateBossHealth(float current, float maxHealth)
    {
        float fillAmount = Mathf.Clamp01(current / maxHealth);
        if (_bossHealthBarImage != null) {
            if (fillAmount == 1)  _bossHealthBarImage.fillAmount = fillAmount;
            else _bossHealthBarImage.DOFillAmount(fillAmount, 0.5f).SetEase(Ease.OutQuad);
            _bossHealthText.gameObject.SetActive(true);
            _bossHealthText.text = $"{current}/{maxHealth}";
        }
    }

    public void ShowIntroBossHealth(float duration)
    {
        ShowBossHealthBar(true);
        _bossHealthText.gameObject.SetActive(false);
        _bossHealthBarImage.fillAmount = 0;
        _bossHealthBarImage.DOFillAmount(1, duration).SetEase(Ease.Linear);
    }

    public void ShowBossHealthBar(bool isShow)
    {
        _bossHealthBar.gameObject.SetActive(isShow);
    }

    public void ResetLevel() {
        UpdateLevelUI(1);
        UpdateXpBar(0, _baseXP);
    }

    public void SetTimeText(string time)
    {
        _timeText.text = time;
        _countdownText.text = time;
    }

    public void SetWarningText(string status)
    {
        if (_statusText != null && gameObject.activeInHierarchy) {
            _statusText.text = status;
            StartCoroutine(ShowAndHideWarningText());
        }
    }

    private IEnumerator ShowAndHideWarningText()
    {
        if (_statusText == null) yield break;

        // Show status text
        _statusText.gameObject.SetActive(true);
        _statusText.color = new Color(_statusText.color.r, _statusText.color.g, _statusText.color.b, 1f);
        
        // Wait for duration
        yield return new WaitForSeconds(_statusTextDuration);
        
        // Fade out status text
        float fadeTime = 1f / _statusTextFadeSpeed;
        float elapsedTime = 0f;
        Color startColor = _statusText.color;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeTime);
            _statusText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        // Hide status text
        _statusText.gameObject.SetActive(false);
    }

    public void UpdateAllScoreBoard()
    {
        var runner = NetworkRunnerHandler.Instance.Runner;
        if (runner == null) return;

        List<PlayerStatus> players = new();

        foreach (PlayerRef player in runner.ActivePlayers)
        {
            var obj = runner.GetPlayerObject(player);
            if (obj == null) continue;

            var status = obj.GetComponent<PlayerStatus>();
            players.Add(status);
        }
        Debug.Log("players.Count: " + players.Count);
        players = players.OrderByDescending(p => p.Kills).ToList();

        for (int i = 0; i < _scoreBoardItemList.Count; i++)
        {
            if (i < players.Count)
            {
                var status = players[i];
                _scoreBoardItemList[i].SetData(i + 1, status.PlayerName, status.Kills, status.Deaths);
                _scoreBoardItemList[i].gameObject.SetActive(true);
            }
            else
            {
                _scoreBoardItemList[i].gameObject.SetActive(false);
            }
        }
    }

    public void AddPlayerToScoreBoard(string playerName, int kills, int deaths)
    {
        GameObject line = Instantiate(_scoreBoardLinePrefab, _scoreBoardContent);
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

    public void ShowScoreBoard(bool active)
    {
        _scoreBoardPanel.SetActive(active);
    }

    public void StartFireballCooldown(float duration)
    {
        StartCooldownRoutine("FireballCooldownRoutine", _fireBallCoolDown, _fireBallCoolDownText, duration);
    }

    public void StartRunCooldown(float duration)
    {
        StartCooldownRoutine("RunCooldownRoutine", _runCoolDown, _runCoolDownText, duration);
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

    public void ShowReSpawnTime(string respawnMess)
    {
        _respawnCountdownPanel.SetActive(!string.IsNullOrEmpty(respawnMess));
        _respawnCountdownText.text = respawnMess;
    }

    public void UpdateKillText(int kill) {
        _killText.gameObject.SetActive(true);
        _killText.text = $"{kill}";
    }

    public void UpdateDeathText(int death) {
        _deathText.gameObject.SetActive(true);
        _deathText.text = $"{death}";
    }
}
