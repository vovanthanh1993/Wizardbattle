using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _currentHealth;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _attackDamage = 20f;
    [SerializeField] private float _attackRange = 2f;
    [SerializeField] private float _attackCooldown = 2f;
    
    [Header("Components")]
    [SerializeField] private NavMeshAgent _navAgent;
    [SerializeField] private Animator _animator;
    
    // Private variables
    private Transform _player;
    private bool _isAttacking = false;
    private bool _isDead = false;
    private float _lastAttackTime;
    
    // Animation hashes
    private int _isWalkingHash;
    private int _isAttackingHash;
    private int _isDeadHash;

    private GameObject _playerObj;
    
    private void Awake()
    {
        // Get components
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        
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
    
    private void Start()
    {
        _currentHealth = _maxHealth;
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
    
    private void MoveTowardsPlayer()
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
    
    
    private void UpdateAnimations()
    {
        if (_animator == null) return;
        
        bool isWalking = _navAgent.velocity.magnitude > 0.1f && !_isAttacking;
        _animator.SetBool(_isWalkingHash, isWalking);
    }
    
    public void TakeDamage(float damage)
    {
        if (_isDead) return;
        
        _currentHealth -= damage;
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        _isDead = true;
        
        // Stop movement
        _navAgent.ResetPath();
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
        
        // Destroy after delay
        Destroy(gameObject, 3f);
    }
}
