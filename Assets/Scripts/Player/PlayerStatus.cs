using Fusion;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : NetworkBehaviour
{

    [Header("Networked Data")]
    [OnChangedRender(nameof(OnPlayerNameChanged))]
    [Networked] public NetworkString<_16> PlayerName { get; set; }
    
    [Networked] public int Kills { get; set; }
    [Networked] public int Deaths { get; set; }

    [Networked] public bool IsWin { get; set; } = false;

    private PlayerHealth _playerHealth;

    [Networked] public long XP { get; set; }
    [Networked] public int Level { get; set; } = 1;

    private PlayerAnimation _playerAnimation;

    private PlayerController _playerController;

    public float Damage { get; set; }
    public float Ammor { get; set; }

    [SerializeField] private TMP_Text _playerNameText;

    public override void Spawned()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerController = GetComponent<PlayerController>();
        UpdatePlayerName();
        if (Object.HasInputAuthority) 
        {
            FirebaseDataManager.Instance.BuyFood(0, -1);
            Damage = FirebaseDataManager.Instance.GetCurrentUserDamage();
            Ammor = FirebaseDataManager.Instance.GetCurrentUserAmmor();
        }
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
        if(Object.HasInputAuthority) {
            _playerNameText.gameObject.SetActive(false);
        } else {
            _playerNameText.gameObject.SetActive(true);
        }
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
        //GameManager.Instance.RpcUpdateLeaderboard();
    }
    
    #endregion

    #region Health Management
    
    

    public void ResetPlayer()
    {
        if (Object.HasInputAuthority)
        {
            ResetPlayerRpc();
        }
    }

    

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void ResetPlayerRpc()
    {
        _playerController.ResetState();
        _playerHealth?.ResetHealth();
    }

    public void AddKill()
    {
        Kills++;
        AddXP(50);
        StartCoroutine(RunDelayedLeaderboardUpdate());
    }

    public void AddXP(long amount)
    {
        XP += amount;
        RpcPlayUpdateLevelUI();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RpcPlayUpdateLevelUI()
    {
        UIManager.Instance.UpdateLevelUI(XP);
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
        _playerHealth.UpdateHealthBarBillboard();
    }

    private void UpdatePlayerNameBillboard()
    {
        if (_playerNameText != null)
        {
            _playerNameText.transform.LookAt(Camera.main.transform);
            _playerNameText.transform.Rotate(0, 180, 0);
        }
    }
    
    #endregion
} 