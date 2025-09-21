using UnityEngine;
using System.Collections;

public class DragonForestController : EnemyController
{
    [Header("Dragon Forest Settings")]

    [SerializeField] private GameObject _fireballPivot;
    protected new void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            _enemyHealth.TakeDamage(_enemyHealth.GetMaxHealth());
        }

        if (_isDead) return;

        if (_playerObj == null) _playerObj = GameObject.FindGameObjectWithTag("Player");
        if (_playerObj != null)
        {
            _player = _playerObj.transform;  
        }
        
        if (_player != null)
        {
            MoveTowardsPlayer();
            CheckFireballAttack();
        }
        
        UpdateAnimations();
    }
    
    private void CheckFireballAttack()
    {
        if (_player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        
        // Nếu trong tầm tấn công và cooldown đã hết thì bắn fireball
        if (distanceToPlayer <= _attackRange && Time.time - _lastAttackTime >= _attackCooldown)
        {
            StartCoroutine(PerformFireballAttack());
        }
    }
    
    private IEnumerator PerformFireballAttack()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;
        
        // Play attack animation and shootevent
        if (_animator != null)
        {
            _animator.SetTrigger(_isAttackingHash);
        }
        
        // Wait for attack animation
        yield return new WaitForSeconds(0.5f);
        
        _isAttacking = false;
    }
    
    private void ShootFireballAtPlayer()
    {
        if (_player == null) return;
        
        // Lấy fireball từ pool
        DragonForestFireBall fireball = GamePoolManager.Instance?.GetDragonForestFireball();
        if (fireball == null) return;
        
        // Tính toán hướng bắn
        Vector3 direction = (_player.position - _fireballPivot.transform.position).normalized;
        
        // Thiết lập vị trí spawn fireball (từ miệng rồng)
        Vector3 spawnPosition = _fireballPivot.transform.position;
        
        // Khởi tạo fireball
        fireball.transform.position = spawnPosition;
        fireball.Init(direction);
        
        // Xoay fireball theo hướng bắn
        if (direction != Vector3.zero)
        {
            fireball.transform.rotation = Quaternion.LookRotation(direction);
            fireball.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }

    public override void Die()
    {
        base.Die();
        GameStatusManager.Instance.SetGameStatus(GameStatus.ENDGAME);
        UIManager.Instance.GamePlayPanel.ShowBossHealthBar(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            var playerStatus = playerObj.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.SetDisable(true);
            }
        }
        // Stop spawning enemies
        Destroy(EnemySpawner.Instance.gameObject);
        // Kill all enemies when DragonForest dies  
        EnemyPoolManager.Instance.KillAllEnemies();
        // Show victory popup after 2 seconds
        PvpResultPopup.Instance.ShowVictoryPopup(2f);
    }
}
