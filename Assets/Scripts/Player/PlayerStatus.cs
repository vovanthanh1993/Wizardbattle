using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private GameObject _pivotHealthBar;

    [Header("Networked Data")]
    [OnChangedRender(nameof(OnPlayerNameChanged))]
    [Networked] public NetworkString<_16> PlayerName { get; set; }
    
    [Networked] public int Kills { get; set; }
    [Networked] public int Deaths { get; set; }
    [Networked] public bool IsDead { get; set; }
    [Networked] public bool IsDisable { get; set; }

    [Networked] public bool IsWin { get; set; } = false;

    private PlayerHealth _playerHealth;

    [Networked] public int Coin { get; set; }

    private PlayerAnimation _playerAnimation;

    public override void Spawned()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        UpdatePlayerName();
    }

    #region Player Name Management
    
    public void SetPlayerName(string name)
    {
        if (Object.HasInputAuthority)
        {
            SetPlayerNameRpc(name);
        }
    }

    private void UpdatePlayerName()
    {
        if(Object.HasInputAuthority) _playerNameText.gameObject.SetActive(false);
        if (_playerNameText != null)
        {
            _playerNameText.text = PlayerName.ToString();
        }
    }

    private void OnPlayerNameChanged()
    {
        UpdatePlayerName();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void SetPlayerNameRpc(string name)
    {
        PlayerName = name;
        StartCoroutine(RunDelayedLeaderboardUpdate());
    }

    private IEnumerator RunDelayedLeaderboardUpdate()
    {
        yield return new WaitForFixedUpdate();
        GameManager.Instance.RpcUpdateLeaderboard();
    }
    
    #endregion

    #region Health Management
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        if (_healthBarImage != null)
        {
            _healthBarImage.fillAmount = fillAmount;
        }
    }

    public void ResetPlayer()
    {
        if (Object.HasInputAuthority)
        {
            ResetPlayerRpc();
        }
    }

    public void Heal(int healAmount)
    {
        _playerHealth?.Heal(healAmount);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void ResetPlayerRpc()
    {
        IsDead = false;
        _playerHealth?.ResetHealth();
    }
    
    #endregion

    #region Game State Management
    
    public void SetDisable(bool isDisable)
    {
        if (Object.HasStateAuthority)
        {
            IsDisable = isDisable;
        }
    }

    public void SetDead(bool isDead)
    {
        if (Object.HasStateAuthority)
        {
            IsDead = isDead;
        }
    }

    public void AddKill()
    {
        Kills++;
        StartCoroutine(RunDelayedLeaderboardUpdate());
    }

    public void AddCoin(int amount)
    {
        Coin += amount;
        RpcPlayUpdateCoinText();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RpcPlayUpdateCoinText()
    {
        UIManager.Instance.UpdateCoinText(Coin);
    }

    public void AddDeath()
    {
        Deaths++;
        StartCoroutine(RunDelayedLeaderboardUpdate());
    }
    
    #endregion

    #region UI Updates
    
    public void UpdateUIElements()
    {
        UpdatePlayerNameBillboard();
        UpdateHealthBarBillboard();
    }

    private void UpdatePlayerNameBillboard()
    {
        if (_playerNameText != null)
        {
            _playerNameText.transform.LookAt(Camera.main.transform);
            _playerNameText.transform.Rotate(0, 180, 0);
        }
    }

    private void UpdateHealthBarBillboard()
    {
        if (_pivotHealthBar != null)
        {
            _pivotHealthBar.transform.LookAt(Camera.main.transform);
            _pivotHealthBar.transform.Rotate(0, 180, 0);
        }
    }
    
    #endregion
} 