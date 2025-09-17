using System.Collections;
using Fusion;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    [OnChangedRender(nameof(OnCurrentHealthChanged))]
    [Networked] public int CurrentHealth { get; set; }

    [Networked] public bool IsDead { get; set; }

    public int MaxHealth = 1000;
    private int _lastHealth;

    public override void Spawned()
    {
        MaxHealth = FirebaseDataManager.Instance.GetCurrentPlayerData().health;
        Debug.Log("MaxHealth: " + MaxHealth);
        ResetHealth();
        if (Object.HasInputAuthority)
        {
            UIManager.Instance?.UpdateHealth(CurrentHealth, MaxHealth);
            _lastHealth = CurrentHealth;
        } else GetComponent<PlayerController>()?.UpdateHealthBar(CurrentHealth, MaxHealth);
    }

    public override void FixedUpdateNetwork()
    {
        /*if (Object.HasInputAuthority && CurrentHealth != _lastHealth)
        {
            UIManager.Instance?.UpdateHealth(CurrentHealth, MaxHealth);
            _lastHealth = CurrentHealth;
        }*/
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
            playerStatus.SetDead(true);
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

    public void Heal(int health)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + health, MaxHealth);
    }
    private void OnCurrentHealthChanged()
    {
        if (Object.HasInputAuthority)
        {
            UIManager.Instance?.UpdateHealth(CurrentHealth, MaxHealth);
        }
        else
        {
            GetComponent<PlayerController>()?.UpdateHealthBar(CurrentHealth, MaxHealth);
        }
    }

    public void ResetHealth()
    {
        if (Object.HasStateAuthority)
        {
            CurrentHealth = MaxHealth;
            IsDead = false;
            var playerStatus = GetComponent<PlayerStatus>();
            playerStatus?.SetDead(false);
        }
    }

    public bool IsAlive => CurrentHealth > 0 && !IsDead;
    public float HealthPercentage => (float)CurrentHealth / MaxHealth;
}