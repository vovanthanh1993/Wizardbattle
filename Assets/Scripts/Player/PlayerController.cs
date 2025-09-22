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

    #region Properties
    
    private bool _isDead;
    private bool _isDisable;
    public string PlayerName => _playerStatus?.PlayerName.ToString() ?? "";
    public int Kills => _playerStatus?.Kills ?? 0;
    public int Deaths => _playerStatus?.Deaths ?? 0;

    #endregion

    #region Private Fields
    
    private Vector2 _baseLookRotation;
    private float _timeRemaining;
    private bool _isRespawning = false;
    
    // Component References
    private PlayerStatus _playerStatus;
    private PlayerAnimation _playerAnimation;
    private PlayerHealth _playerHealth;
    private float _nextFireTime;
    private float _nextJumpTime;
    private float _nextHealTime;
    private float _nextStealthTime;

    private GameObject _kccCollider;
    private Rigidbody _rb;
    public override void Spawned()
    {
        InitializeComponents();
        SetupInputAuthority();
        Transform kccTransform = transform.Find("KCCCollider");
        if (kccTransform != null)
        {
            _kccCollider = kccTransform.gameObject;
        }
        _rb = GetComponent<Rigidbody>();
    }

    public override void FixedUpdateNetwork()
    {
        if (_isDisable) {
            _kcc.enabled = false;
            return;
        }

        HandleRespawn();

        if (_isRespawning || _isDead) return;

        HandleInput();
    }

    public override void Render()
    {
        if (!_isDisable)
        {
            UpdateCameraTarget();
        }
        _playerAnimation?.UpdateAnimations();
        //_playerAnimation?.HandlePlayerDead();
    }

    public override void Despawned(NetworkRunner runner, bool hasStateChanged)
    {
        CleanupInputAuthority();
    }

    private void LateUpdate()
    {
        if (!_isDisable)
        {
            // Only update UI every few frames to reduce AABB calculations
            if (Time.frameCount % 3 == 0) // Update every 3rd frame
            {
                _playerStatus?.UpdateUIElements();
            }
        }
    }
    
    #endregion

    #region Initialization
    
    private void InitializeComponents()
    {
        _kcc = GetComponent<KCC>();
        _playerStatus = GetComponent<PlayerStatus>();
        _playerAnimation = GetComponent<PlayerAnimation>();
        _playerHealth = GetComponent<PlayerHealth>();
    }

    private void SetupInputAuthority()
    {
        if (!Object.HasInputAuthority) return;
        
        string playerName = UIManager.Instance.GetPlayerName();
        _playerStatus?.SetPlayerName(playerName);

        SetupCamera();
    }

    private void SetupCamera()
    {
        if (_isDisable) return;
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
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Jump) && Object.HasInputAuthority && _kcc.FixedData.IsGrounded && !_isDisable && UIManager.Instance.GamePlayPanel.IsEnableSkill2)
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
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Fire) && Object.HasInputAuthority && !_isDisable)
        {
            Shoot();
        }
    }

    private void HandleSkillHeal(NetworkInputData input)
    {
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Heal) && Object.HasInputAuthority && !_isDisable && UIManager.Instance.GamePlayPanel.IsEnableSkill1)
        {
            RpcHeal();
        }
    }

    private void HandleSkillStealth(NetworkInputData input)
    {
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Stealth) && Object.HasInputAuthority && !_isDisable && UIManager.Instance.GamePlayPanel.IsEnableSkill3)
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
        if (_isDisable) return;
        
        _kcc.AddLookRotation(input.LookDelta * _lookSensitivity, _minPitch, _maxPitch);
        _baseLookRotation = _kcc.GetLookRotation();
    }

    private void HandleMovement(NetworkInputData input)
    {
        if (_isDisable) return;
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
        if (_isDisable) return;
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

        RpcSpawnFireBallLocal(start, direction);

        _playerAnimation?.TriggerShoot();
        _nextFireTime = Runner.SimulationTime + _fireRate;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcHeal()
    {
        if (Runner.SimulationTime < _nextHealTime) return;
        _playerHealth.Heal(_healAmount);
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
    public void RpcSpawnFireBallLocal(Vector3 position, Vector3 direction)
    {
        FireBall fireball = GetFireBallFromPool(position, direction);
        if (fireball != null)
        {
            fireball.Init(direction, Object);
            AudioManager.Instance.PlayFireballSoundAtPosition(_firePoint.position);
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
        if (_isDead && !_isRespawning && Object.HasInputAuthority && !_isDisable)
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
        Transform spawnPoint = PlayerSpawnManager.Instance.GetSpawnPoint();
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

    #region Public Methods
    
    public void HandlePlayerHurt()
    {
        _playerAnimation?.TriggerHurt();
        RpcPlayPlayerHitSound();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    public void RpcPlayPlayerHitSound()
    {
        AudioManager.Instance.PlayPlayerHitSound();
    }

    public void SetDisable(bool isDisable)
    {
        _isDisable = isDisable;
        _kcc.enabled = !isDisable;
        _kccCollider.SetActive(false);
        if (_rb != null) {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        if (Object.HasInputAuthority)
        {
            CameraController.Instance.SetTarget(null);
        }
            
        SetIdleAnimation();
    }

    public void SetIdleAnimation()
    {
        _playerAnimation?.SetIdleAnimation();
    }

    public void Die() {
        _isDead = true;
        _isDisable = true;
        _kcc.enabled = false;
        _kccCollider.SetActive(false);

        if (_rb != null) {
            _rb.isKinematic = true;
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        _playerAnimation.Die();

        if (NetworkRunnerHandler.Instance.GameType == GameType.PVE) {
            PvpResultPopup.Instance.ShowLosePopup(2f);
        } else {
            StartCoroutine(HideModelAfterDelay(1f));
        }
    }

    private IEnumerator HideModelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _playerAnimation.HandlePlayerDead();
    }

    public bool IsDead() {
        return _isDead;
    }

    public void Reset() {
        _isDead = false;
        _isDisable = false;
        _kcc.enabled = true;
        _kccCollider.SetActive(true);
        CameraController.Instance.SetTarget(_camTarget);
        _playerAnimation.ResetState();
    }
    #endregion
}