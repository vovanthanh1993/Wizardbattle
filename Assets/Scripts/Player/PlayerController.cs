using Fusion;
using Fusion.Addons.KCC;
using System.Collections;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    #region Serialized Fields
    
    [Header("Movement Settings")]
    [SerializeField] private KCC _kcc;
    [SerializeField] private float _maxPitch = 60f; // Giảm từ 85f xuống 60f
    [SerializeField] private float _minPitch = -25f; // Giới hạn nhìn xuống (25 độ)
    [SerializeField] private float _lookSensitivity = 0.15f;
    [SerializeField] private Vector3 _jumpImpulse = new(0f, 10f, 0f);
    
    [Header("Combat Settings")]
    [SerializeField] private float _fireRate = 10f;
    [SerializeField] private float _jumpRate = 40f;
    [SerializeField] private float _healRate = 60f;
    [SerializeField] private int _healAmount = 30;

    [SerializeField] private int  _stealthRate = 30;
    [SerializeField] private int  _stealthDuration = 5;
    [SerializeField] private GameObject _fireBallPrefab;
    [SerializeField] private Transform _firePoint;
    
    [Header("Camera Settings")]
    [SerializeField] private Transform _camTarget;
    
    [Header("Respawn Settings")]
    [SerializeField] private float _respawnTime = 3f;
    
    #endregion

    #region Networked Properties
    
    [Networked] private NetworkButtons _previousButtons { get; set; }
    
    #endregion

    #region Private Fields
    
    private Vector2 _baseLookRotation;
    private float _timeRemaining;
    private bool _isRespawning = false;
    
    // Component References
    private PlayerStatus _playerStatus;
    private PlayerAnimation _playerAnimation;
    
    private float _nextFireTime;
    private float _nextJumpTime;
    private float _nextHealTime;
    private float _nextStealthTime;
    public override void Spawned()
    {
        InitializeComponents();
        SetupInputAuthority();
    }

    public override void FixedUpdateNetwork()
    {
        if (IsDisable) {
            _kcc.enabled = false;
            return;
        }

        HandleRespawn();

        if (_isRespawning || IsDead) return;

        HandleInput();
    }

    public override void Render()
    {
        if (!IsDisable)
        {
            UpdateCameraTarget();
        }
        _playerAnimation?.UpdateAnimations();
        _playerAnimation?.HandlePlayerDead();
    }

    public override void Despawned(NetworkRunner runner, bool hasStateChanged)
    {
        CleanupInputAuthority();
    }

    private void LateUpdate()
    {
        if (!IsDisable)
        {
            _playerStatus?.UpdateUIElements();
        }
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        _kcc = GetComponent<KCC>();
        _playerStatus = GetComponent<PlayerStatus>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    private void SetupInputAuthority()
    {
        if (!Object.HasInputAuthority) return;
        
        string playerName = UIManager.Instance.GetPlayerName();
        _playerStatus?.SetPlayerName(playerName);
        _playerStatus.IsDisable = false;

        SetupCamera();
    }

    private void SetupCamera()
    {
        _camTarget.gameObject.SetActive(true);
        CameraController.Instance.SetTarget(_camTarget);
        _kcc.Settings.ForcePredictedLookRotation = true;
    }

    private void CleanupInputAuthority()
    {
        if (!Object.HasInputAuthority) return;
        
        CameraController.Instance.SetTarget(null);
        _camTarget.gameObject.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    #endregion

    #region Input Handling
    
    private void HandleInput()
    {
        if (!GetInput(out NetworkInputData input)) return;
        
        HandleJump(input);
        HandleShoot(input);
        HandleSkillHeal(input);
        HandleSkillStealth(input);
        HandleLookRotation(input);
        HandleMovement(input);
        UpdatePreviousInput(input);
    }

    private void HandleJump(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Jump) && Object.HasInputAuthority && _kcc.FixedData.IsGrounded && !IsDisable)
        { 
            RpcJump();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RpcJump()
    {
        if (Runner.SimulationTime < _nextJumpTime) return;
        
        _kcc.Jump(_jumpImpulse);
        _nextJumpTime = Runner.SimulationTime + _jumpRate;
        
        // Chỉ player có InputAuthority mới update UI
        if (Object.HasInputAuthority)
        {
            UIManager.Instance.StartJumpCooldown(_jumpRate);
        }
    }

    private void HandleShoot(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Fire) && Object.HasInputAuthority && !IsDisable)
        {
            Shoot();
        }
    }

    private void HandleSkillHeal(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Heal) && Object.HasInputAuthority && !IsDisable)
        {
            RpcHeal();
        }
    }

    private void HandleSkillStealth(NetworkInputData input)
    {
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Stealth) && Object.HasInputAuthority && !IsDisable)
        {
            RpcStealth();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcStealth()
    {
        if (Runner.SimulationTime < _nextStealthTime) return;
        
        _nextStealthTime = Runner.SimulationTime + _stealthRate;
        if (Object.HasInputAuthority) UIManager.Instance.StartStealthCooldown(_stealthRate);
        
        // Ẩn model trong 5 giây sử dụng PlayerAnimation
        StartCoroutine(StealthCoroutine());
        
        if (AudioManager.Instance != null)
        {
            //AudioManager.Instance.PlayHealSound();
        }
    }

    private IEnumerator StealthCoroutine()
    {
        _playerAnimation.SetModelVisibility(false);
        yield return new WaitForSeconds(_stealthDuration);
        _playerAnimation.SetModelVisibility(true);
    }
    private void HandleLookRotation(NetworkInputData input)
    {
        _kcc.AddLookRotation(input.LookDelta * _lookSensitivity, _minPitch, _maxPitch);
        _baseLookRotation = _kcc.GetLookRotation();
    }

    private void HandleMovement(NetworkInputData input)
    {
        Vector3 worldDirection = _kcc.FixedData.TransformRotation * input.Direction.X0Y();
        _kcc.SetInputDirection(worldDirection);

        if (Object.HasStateAuthority)
        {
            float moveSpeed = worldDirection.magnitude;
            _playerAnimation?.SetMoveSpeed(moveSpeed);
        }
    }

    private void UpdatePreviousInput(NetworkInputData input)
    {
        _previousButtons = input.Buttons;
    }
    
    #endregion

    #region Combat
    
    public void Shoot()
    {
        if (Runner.SimulationTime < _nextFireTime) return;

        UIManager.Instance.StartFireballCooldown(_fireRate);
        // Get camera direction and start position
        Vector3 cameraDirection = GetCameraDirection();
        Vector3 start = GetFireballStartPosition();
        Vector3 direction = cameraDirection;
        float speed = 25f;
        float lifetime = 3f;

        RpcSpawnFireBallLocal(start, direction, speed, lifetime);

        _playerAnimation?.TriggerShoot();
        _nextFireTime = Runner.SimulationTime + _fireRate;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFireballSound();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcHeal()
    {
        if (Runner.SimulationTime < _nextHealTime) return;
        _playerStatus?.Heal(_healAmount);
        _nextHealTime = Runner.SimulationTime + _healRate;
        if (Object.HasInputAuthority) UIManager.Instance.StartHealingCooldown(_healRate);
        if (AudioManager.Instance != null)
        {
            //AudioManager.Instance.PlayHealSound();
        }
    }
    
    private Vector3 GetCameraDirection()
    {
        // Get the camera's forward direction
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            Vector3 cameraDirection = mainCamera.transform.forward;
            return cameraDirection.normalized;
        }
        
        // Fallback to firePoint direction if camera not found
        return _firePoint.forward;
    }
    
    private Vector3 GetFireballStartPosition()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // Always start from player position (firePoint) but use camera direction
            Vector3 cameraDirection = GetCameraDirection();
            return _firePoint.position + cameraDirection * 1.5f;
        }
        
        // Fallback to firePoint position
        return _firePoint.position + _firePoint.forward * 1.5f;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcSpawnFireBallLocal(Vector3 position, Vector3 direction, float speed, float lifetime)
    {
        FireBall fireball = GetFireBallFromPool(position, direction);
        
        if (fireball != null)
        {
            fireball.Init(direction, speed, lifetime, Object);
        }
    }

    private FireBall GetFireBallFromPool(Vector3 position, Vector3 direction)
    {
        FireBall fireball = null;
        
        if (GamePoolManager.Instance != null)
        {
            fireball = GamePoolManager.Instance.GetFireBall();
        }
        else
        {
            var fireballObj = Instantiate(_fireBallPrefab, position, Quaternion.LookRotation(direction));
            fireball = fireballObj.GetComponent<FireBall>();
        }
        
        if (fireball != null)
        {
            fireball.transform.position = position;
            fireball.transform.rotation = Quaternion.LookRotation(direction);
        }
        
        return fireball;
    }
    
    #endregion

    #region Respawn System
    
    private void HandleRespawn()
    {
        if (IsDead && !_isRespawning && Object.HasInputAuthority && !IsDisable)
        {
            StartRespawn();
        }
    }

    private void StartRespawn()
    {
        _isRespawning = true;
        _timeRemaining = _respawnTime;
        if (Object.HasInputAuthority)
        {
            StartCoroutine(HandleRespawnCountdown());
        }
    }

    private IEnumerator HandleRespawnCountdown()
    {
        while (_timeRemaining > 0)
        {
            UIManager.Instance.ShowReSpawnTime(string.Format(GameConstants.RESPAWN_FORMAT, Mathf.Ceil(_timeRemaining).ToString()));
            _timeRemaining -= Time.deltaTime;
            yield return null;
        }

        CompleteRespawn();
    }

    private void CompleteRespawn()
    {
        Transform spawnPoint = GameManager.Instance.GetSpawnPoint();
        _kcc.TeleportRPC(spawnPoint.position, spawnPoint.rotation.eulerAngles.x, spawnPoint.rotation.eulerAngles.y);
        _playerStatus?.ResetPlayer();
        UIManager.Instance.ShowReSpawnTime("");
        StartCoroutine(FinishRespawn());
    }

    private IEnumerator FinishRespawn()
    {
        yield return new WaitForSeconds(1f);
        _isRespawning = false;
    }
    
    #endregion

    #region Visual & Camera
    
    private void UpdateCameraTarget()
    {
        _camTarget.localRotation = Quaternion.Euler(_kcc.GetLookRotation().x, 0f, 0f);
    }
    
    #endregion

    #region Properties
    
    public bool IsDead => _playerStatus?.IsDead ?? false;
    public bool IsDisable => _playerStatus?.IsDisable ?? false;
    public string PlayerName => _playerStatus?.PlayerName.ToString() ?? "";
    public int Kills => _playerStatus?.Kills ?? 0;
    public int Deaths => _playerStatus?.Deaths ?? 0;
    
    #endregion

    #region Public Methods
    
    public void HandlePlayerHurt()
    {
        _playerAnimation?.TriggerHurt();
        AudioManager.Instance?.PlayPlayerHitSound();
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        _playerStatus?.UpdateHealthBar(currentHealth, maxHealth);
    }

    public void SetDisable(bool isDisable)
    {
        _playerStatus?.SetDisable(isDisable);
    }

    public void SetIdleAnimation()
    {
        _playerAnimation?.SetIdleAnimation();
    }
    
    #endregion
}