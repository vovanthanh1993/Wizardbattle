using Fusion;
using UnityEngine;

public class PlayerAnimation : NetworkBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private MeshRenderer[] _modelParts;
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _model;

    [Header("Networked Animation States")]
    [Networked] private bool _didShoot { get; set; }
    [Networked] private float _moveSpeed { get; set; }
    [Networked] private bool _hurt { get; set; }
    [Networked] private bool _isStealth { get; set; }
    [Networked] private bool _isDead { get; set; }


    private GameObject _cachedKCCCollider;

    public override void Spawned()
    {
        SetupModelRendering();
    }

    #region Model Setup
    
    private void SetupModelRendering()
    {
        if (!Object.HasInputAuthority) return;
        
        foreach (MeshRenderer renderer in _modelParts)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
    }
    
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

    #region Visual State Management
    
    public void HandlePlayerDead()
    {
        if (_model != null)
        {
            // Chỉ hiện model khi không phải stealth mode và không chết
            bool shouldShowModel = !_isDead && !_isStealth;
            _model.SetActive(shouldShowModel);
        }
        if (_cachedKCCCollider == null)
        {
            var found = transform.Find("KCCCollider");
            if (found != null) _cachedKCCCollider = found.gameObject;
        }
        if (_cachedKCCCollider != null)
        {
            if (_isDead)
            {
                _cachedKCCCollider.layer = LayerMask.NameToLayer("IgnorePlayerCollision");
                var rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
            else
            {
                _cachedKCCCollider.layer = LayerMask.NameToLayer("Default");
                var rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;
            }
        }
    }
    public void SetModelVisibility(bool visible) {
        _isStealth = !visible; // true khi ẩn, false khi hiện
        
        if (_model != null)
        {
            _model.SetActive(visible);
        }
    }

    #endregion

    public void Die(){
        _isDead = true;
    }

    public void Reset(){
        _isDead = false;
    }
} 