using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] protected float _moveSpeed = 4f;
    [SerializeField] protected float _attackDamage = 20f;
    [SerializeField] protected float _attackRange = 2f;
    [SerializeField] protected float _attackCooldown = 2f;
    
    [Header("Components")]
    [SerializeField] protected NavMeshAgent _navAgent;
    [SerializeField] protected Animator _animator;

    [SerializeField] protected EnemyHealth _enemyHealth;

    [SerializeField] protected float _xpDropChance = 0.9f;
    [SerializeField] protected float _xpDropChanceRed = 0.2f;

    [SerializeField] protected float _healthDropChance = 0.1f;
    
    // Private variables
    protected Transform _player;
    protected bool _isAttacking = false;
    protected bool _isDead = false;
    protected float _lastAttackTime;
    
    // Animation hashes
    protected int _isWalkingHash;
    protected int _isAttackingHash;
    protected int _isDeadHash;

    protected GameObject _playerObj;
    
    private void Awake()
    {
        // Get components
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _enemyHealth = GetComponent<EnemyHealth>();
        
        // Set up nav agent
        if (_navAgent != null)
        {
            _navAgent.speed = _moveSpeed;
            _navAgent.stoppingDistance = _attackRange;
        }
        
        // Cache animation hashes
        _isWalkingHash = Animator.StringToHash("IsWalking");
        _isAttackingHash = Animator.StringToHash("IsAttacking");
        _isDeadHash = Animator.StringToHash("IsDead");
    }
    
    private void OnEnable()
    {
        // Reset state when enabled (spawned from pool)
        OnSpawn();
    }
    
    private void Update()
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
            CheckAttack();
        }
        
        UpdateAnimations();
    }
    
    protected void MoveTowardsPlayer()
    {
        if (_player == null) return;
        
        // Luôn di chuyển về phía player
        _navAgent.SetDestination(_player.position);
        
        // Look at player
        Vector3 direction = (_player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
    
    private void CheckAttack()
    {
        if (_player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        
        // Nếu trong tầm tấn công và cooldown đã hết thì tấn công
        if (distanceToPlayer <= _attackRange && Time.time - _lastAttackTime >= _attackCooldown)
        {
            StartCoroutine(PerformAttack());
        }
    }
    
    private IEnumerator PerformAttack()
    {
        _isAttacking = true;
        _lastAttackTime = Time.time;
        
        // Play attack animation
        if (_animator != null)
        {
            _animator.SetTrigger(_isAttackingHash);
        }
        
        // Wait for attack animation
        yield return new WaitForSeconds(0.5f);
        
        // Deal damage to player
        if (_player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
            if (distanceToPlayer <= _attackRange)
            {
                // Apply damage to player
                PlayerHealth playerHealth = _player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage((int)_attackDamage, null);
                }
            }
        }
        
        _isAttacking = false;
    }
    
    
    protected void UpdateAnimations()
    {
        if (_animator == null) return;
        
        bool isWalking = _navAgent.velocity.magnitude > 0.1f && !_isAttacking;
        _animator.SetBool(_isWalkingHash, isWalking);
    }
    
    public void Die()
    {
        _isDead = true;
        
        // Stop movement
        if (_navAgent != null && _navAgent.isOnNavMesh)
        {
            _navAgent.ResetPath();
        }
        _navAgent.enabled = false;
        
        // Play death animation
        if (_animator != null)
        {
            _animator.SetTrigger(_isDeadHash);
        }
        
        // Disable collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // Return to pool after delay
        StartCoroutine(ReturnToPoolAfterDelay());
    }
    
    private IEnumerator ReturnToPoolAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        SpawnXPItem();

        // Return to pool
        yield return new WaitForSeconds(1f);
        if (EnemyPoolManager.Instance != null)
        {
            EnemyPoolManager.Instance.ReturnEnemy(this);
        }
        else
        {
            // Fallback: destroy if no pool manager
            Destroy(gameObject);
        }
    }

    private void SpawnXPItem()
    {
        // 70% chance to drop XP item
        Vector3 spawnPosition = transform.position + Vector3.up * 1f;
        float chance = Random.Range(0f, 1f);
        if (chance <= _healthDropChance)
        {
            Health health = GamePoolManager.Instance.GetHealth();
            if (health != null)
            {
                health.transform.position = spawnPosition;
                health.gameObject.SetActive(true);
            }
            
        } else if (chance <= _xpDropChanceRed){
            XPItemE xpItem = GamePoolManager.Instance.GetXpItemRed();
            if (xpItem != null)
            {
                xpItem.transform.position = spawnPosition;
                xpItem.gameObject.SetActive(true);
            }
        } else if (chance <= _xpDropChance)
        {
            XPItemE xpItem = GamePoolManager.Instance.GetXpItem();
            if (xpItem != null)
            {
                xpItem.transform.position = spawnPosition;
                xpItem.gameObject.SetActive(true);
            }
        }
    }
    
    // IPoolable implementation
    public void OnSpawn()
    {
        // Reset state when spawned from pool
        _isDead = false;
        _isAttacking = false;
        _lastAttackTime = 0f;
        
        // Reset health
        if (_enemyHealth != null)
        {
            _enemyHealth.Init();
        }
        
        // Reset NavMeshAgent
        if (_navAgent != null)
        {
            _navAgent.enabled = true;
            // Only reset path if agent is on NavMesh
            if (_navAgent.isOnNavMesh)
            {
                _navAgent.ResetPath();
            }
        }
        
        // Reset collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
    }
    
    public void OnReturn()
    {
        // Clean up when returned to pool
        StopAllCoroutines();
    }
    
    public void ResetState()
    {
        // Reset state method for ObjectPool
        OnSpawn();
    }
}
