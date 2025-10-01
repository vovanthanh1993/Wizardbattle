using System.Collections;
using Fusion;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerHealth : NetworkBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private GameObject _pivotHealthBar;
    [SerializeField] private ParticleSystem _healthParticleSystem;

    [OnChangedRender(nameof(OnCurrentHealthChanged))]
    [Networked] public int CurrentHealth { get; set; }

    [Networked] public bool IsDead { get; set; }

    [Networked] public int MaxHealth { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            _pivotHealthBar.gameObject.SetActive(false);
            RpcUpdateData(FirebaseDataManager.Instance.GetCurrentPlayerData().health);
            UIManager.Instance?.UpdateHealth(CurrentHealth, MaxHealth);
        } else {
            _pivotHealthBar.gameObject.SetActive(true);
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        if (_healthBarImage != null)
        {
            _healthBarImage.fillAmount = fillAmount;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcUpdateData(int maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        UpdateHealthBar(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int damage, NetworkObject shooter)
    {
        if (!Object.HasStateAuthority) return;
        if (damage <= 0) return;
        if (IsDead) return;
        
        CurrentHealth = Mathf.Max(CurrentHealth - damage, 0);
        
        var playerController = GetComponent<PlayerController>();
        var playerStatus = GetComponent<PlayerStatus>();
        if (CurrentHealth <= 0 && !IsDead)
        {
            IsDead = true;
            playerController.RpcDie();
            playerStatus.AddDeath();

            if (shooter != null)
            {
                var shooterStatus = shooter.GetComponent<PlayerStatus>();
                
                if (shooterStatus != null && shooterStatus != playerStatus)
                {
                    shooterStatus.AddKill();
                    LobbyManager.Instance.RpcShowKillFeed(shooterStatus.PlayerName.ToString(), playerStatus.PlayerName.ToString());
                }

                if (shooterStatus != null && shooterStatus.Kills >= LobbyManager.Instance.KillsToWin)
                {
                    shooterStatus.IsWin = true;
                    LobbyManager.Instance.EndGame();
                    return;
                }
            }
        } 
        else if (CurrentHealth > 0) 
        {
            playerController.HandlePlayerHurt();
        }
    }

    private void OnCurrentHealthChanged()
    {
        if (Object.HasInputAuthority)
        {
            UIManager.Instance?.UpdateHealth(CurrentHealth, MaxHealth);
        }
        else
        {
            UpdateHealthBar(CurrentHealth, MaxHealth);
        }
    }

    public void ResetHealth()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHealth = MaxHealth;
            IsDead = false;
        }
    }

    public bool IsAlive => CurrentHealth > 0 && !IsDead;
    public float HealthPercentage => (float)CurrentHealth / MaxHealth;

    public void UpdateHealthBarBillboard()
    {
        if (_pivotHealthBar != null)
        {
            _pivotHealthBar.transform.LookAt(Camera.main.transform);
            _pivotHealthBar.transform.Rotate(0, 180, 0);
        }
    }

    public void Heal(int healAmount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + healAmount, MaxHealth);
        AudioManager.Instance.PlayHealthRecoverSound();
        StartCoroutine(PlayHealthParticleEffect());
    }

    private IEnumerator PlayHealthParticleEffect()
    {
        _healthParticleSystem.gameObject.SetActive(true);
        _healthParticleSystem.Play();
        
        yield return new WaitForSeconds(2);

        _healthParticleSystem.gameObject.SetActive(false);
    }
}