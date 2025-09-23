using Fusion;
using Fusion.Addons.KCC;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : NetworkBehaviour
{

    [Header("Networked Data")]
    [Networked] public string PlayerName { get; set; }
    
    [Networked] public int Kills { get; set; }
    [Networked] public int Deaths { get; set; }

    [Networked] public bool IsWin { get; set; } = false;

    private PlayerHealth _playerHealth;

    [Networked] public long XP { get; set; }
    [Networked] public int Level { get; set; } = 1;

    private PlayerAnimation _playerAnimation;

    private PlayerController _playerController;

    [Networked] public float Damage { get; set; }
    [Networked] public float Speed { get; set; }

    

    [SerializeField] private TMP_Text _playerNameText;

    public override void Spawned()
    {
        _playerHealth = GetComponent<PlayerHealth>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerController = GetComponent<PlayerController>();
        if (Object.HasInputAuthority) 
        {
            FirebaseDataManager.Instance.BuyFood(0, -1);
            RpcUpdateData(FirebaseDataManager.Instance.GetCurrentUserDisplayName(), FirebaseDataManager.Instance.GetCurrentUserDamage(), FirebaseDataManager.Instance.GetCurrentUserSpeed()/10);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcUpdateData(string playerName, float damage, float speed)
    {
        Damage = damage;
        Speed = speed;
        PlayerName = playerName;
        if(Object.HasInputAuthority) {
            _playerNameText.gameObject.SetActive(false);
        } else {
             _playerNameText.gameObject.SetActive(true);
             _playerNameText.text = PlayerName;
        }
        UpdateMovementSpeed();
    }

    private void UpdateMovementSpeed()
    {
        var processors = _playerController.GetKCC().LocalProcessors;
        foreach (var processor in processors)
        {
            if (processor is EnvironmentProcessor envProcessor)
            {
                envProcessor.KinematicSpeed = Speed;
                Debug.Log("Speed updated: " + Speed);
                break;
            }
        }
    }

    public void AddKill()
    {
        Kills++;
        AddXP(50);
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
    }

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