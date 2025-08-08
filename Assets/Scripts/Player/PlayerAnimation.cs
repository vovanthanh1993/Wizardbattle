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
        if (!IsDead)
        {
            _hurt = true;
        }
    }

    public void SetMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }
    
    #endregion

    #region Animation Updates
    
    public void UpdateAnimations()
    {
        HandleShootAnimation();
        HandleHurtAnimation();
        HandleMoveSpeedAnimation();
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
    
    #endregion

    #region Visual State Management
    
    public void HandlePlayerDead()
    {
        if (_model != null)
        {
            _model.SetActive(!IsDead);
        }
        if (_cachedKCCCollider == null)
        {
            var found = transform.Find("KCCCollider");
            if (found != null) _cachedKCCCollider = found.gameObject;
        }
        if (_cachedKCCCollider != null)
        {
            if (IsDead)
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
    
    #endregion

    #region Properties
    
    public bool IsDead => GetComponent<PlayerStatus>()?.IsDead ?? false;
    
    #endregion
} 