using Fusion;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private Animator _animator;

    [Header("Networked Animation States")]
    [Networked] private bool _didShoot { get; set; }
    [Networked] private float _moveSpeed { get; set; }
    [Networked] private bool _hurt { get; set; }
    [Networked] private bool _isStealth { get; set; }
    [Networked] private bool _isDead { get; set; }

    #region Model Setup
    
    #endregion

    #region Animation Triggers
    
    public void TriggerShoot()
    {
        _didShoot = true;
    }

    public void TriggerHurt()
    {
        if (!_isDead)
        {
            _hurt = true;
        }
    }

    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    public void SetIdleAnimation()
    {
        _moveSpeed = 0f;
        _didShoot = false;
        _hurt = false;
        _isStealth = false;
        
        if (_animator != null)
        {
            _animator.SetFloat("MoveSpeed", 0f);
            _animator.ResetTrigger("Fire");
            _animator.ResetTrigger("Hurt");
            _animator.ResetTrigger("Die");
        }
    }
    
    #endregion

    #region Animation Updates
    
    public void UpdateAnimations()
    {
        HandleShootAnimation();
        HandleHurtAnimation();
        HandleMoveSpeedAnimation();
        HandleDieAnimation();
    }

    private void HandleShootAnimation()
    {
        if (_didShoot)
        {
            _animator?.SetTrigger("Fire");
            _didShoot = false;
        }
    }

    private void HandleHurtAnimation()
    {
        if (_hurt)
        {
            _animator?.SetTrigger("Hurt");
            _hurt = false;
        }
    }

    private void HandleMoveSpeedAnimation()
    {
        _animator?.SetFloat("MoveSpeed", _moveSpeed);
    }

    private void HandleDieAnimation()
    {
        if (_isDead)
        {
            _animator?.SetTrigger("Die");
            _isDead = false;
        }
    }
    
    #endregion

    public void Die(){
        _isDead = true;
    }

    public void Reset(){
        _isDead = false;
        SetIdleAnimation();
    }
} 