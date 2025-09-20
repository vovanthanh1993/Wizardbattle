using UnityEngine;
using System.Collections;

public class DragonForestController : EnemyController
{
    [Header("Fireball Settings")]
    [SerializeField] private float _fireballSpeed = 10f;
    [SerializeField] private float _fireballLifetime = 3f;
    [SerializeField] private float _fireballDamage = 50f;

    [SerializeField] private GameObject _fireballPivot;
    
    protected new void Update()
    {
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
        FireBall fireball = GamePoolManager.Instance?.GetFireBall();
        if (fireball == null) return;
        
        // Tính toán hướng bắn
        Vector3 direction = (_player.position - _fireballPivot.transform.position).normalized;
        
        // Thiết lập vị trí spawn fireball (từ miệng rồng)
        Vector3 spawnPosition = _fireballPivot.transform.position;
        
        // Khởi tạo fireball
        fireball.transform.position = spawnPosition;
        fireball.Init(direction, _fireballSpeed, _fireballLifetime, null);
        
        // Xoay fireball theo hướng bắn
        if (direction != Vector3.zero)
        {
            fireball.transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
