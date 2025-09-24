using Fusion;
using Fusion.Addons.KCC;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject _model;
    [SerializeField] private float _maxPitch = 85f; // Giảm từ 85f xuống 60f
    [SerializeField] private float _minPitch = -25f; // Giới hạn nhìn xuống (25 độ)
    [SerializeField] private float _lookSensitivity = 0.15f;
    
    [Header("Combat Settings")]
    [SerializeField] private float _fireRate = 10f;
    [SerializeField] private float _runRate = 40f;
    [SerializeField] private float _runAmount = 5f;
    [SerializeField] private float _healRate = 60f;
    [SerializeField] private int _healAmount = 300;

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
    private float _nextRunTime;
    private float _nextHealTime;
    
    private PlayerSkill _playerSkill;
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
        _playerSkill = GetComponent<PlayerSkill>();
    }

    private void SetupInputAuthority()
    {
        if (!Object.HasInputAuthority) return;
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
        
        HandleRun(input);
        HandleShoot(input);
        HandleSkillHeal(input);
        HandleSkillStealth(input);
        HandleLookRotation(input);
        HandleMovement(input);
        UpdatePreviousInput(input);
    }

    private void HandleRun(NetworkInputData input)
    {
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Run) && Object.HasInputAuthority && _kcc.FixedData.IsGrounded && UIManager.Instance.GamePlayPanel.IsEnableSkill2)
        { 
            RpcRun();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RpcRun()
    {
        if (Runner.SimulationTime < _nextRunTime) return;
        
        StartCoroutine(TemporarySpeedBoost());
        _playerSkill.RpcPlayPowerUp(3);
        _nextRunTime = Runner.SimulationTime + _runRate;
        
        if (Object.HasInputAuthority)
        {
            UIManager.Instance.GamePlayPanel.StartRunCooldown(_runRate);
        }
    }

    private void HandleShoot(NetworkInputData input)
    {
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Fire) && Object.HasInputAuthority)
        {
            Shoot();
        }
    }

    private void HandleSkillHeal(NetworkInputData input)
    {
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Heal) && Object.HasInputAuthority && UIManager.Instance.GamePlayPanel.IsEnableSkill1)
        {
            RpcHeal();
        }
    }

    private void HandleSkillStealth(NetworkInputData input)
    {
        if (_isDisable) return;
        if (input.Buttons.WasPressed(_previousButtons, InputButtons.Stealth) && Object.HasInputAuthority && UIManager.Instance.GamePlayPanel.IsEnableSkill3)
        {
            _playerSkill.RpcStealth();
        }
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

    private IEnumerator TemporarySpeedBoost()
    {
        var processors = _kcc.LocalProcessors;
        foreach (var processor in processors)
        {
            if (processor is EnvironmentProcessor envProcessor)
            {
                envProcessor.KinematicSpeed = _playerStatus.Speed + _runAmount;
                break;
            }
        }

        yield return new WaitForSeconds(3f);
        foreach (var processor in processors)
        {
            if (processor is EnvironmentProcessor envProcessor)
            {
                envProcessor.KinematicSpeed = _playerStatus.Speed;
            }
        }
    }
    
    #endregion

    #region Combat
    
    public void Shoot()
    {
        if (Runner.SimulationTime < _nextFireTime) return;

        UIManager.Instance.GamePlayPanel.StartFireballCooldown(_fireRate);
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
        if (Object.HasInputAuthority) UIManager.Instance.GamePlayPanel.StartHealingCooldown(_healRate);
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
            GameStatusManager.Instance.SetGameStatus(GameStatus.ENDGAME);
            PvpResultPopup.Instance.ShowLosePopup(2f);
        } else {
            StartCoroutine(HideModelAfterDelay(1f));
        }
    }

    private IEnumerator HideModelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePlayerModel();
        float countdownTime = 3f;
        while (countdownTime > 0)
        {
            if(Object.HasInputAuthority)
                UIManager.Instance.GamePlayPanel.ShowReSpawnTime(string.Format(GameConstants.RESPAWN_FORMAT, Mathf.Ceil(countdownTime).ToString()));
            countdownTime -= Time.deltaTime;
            yield return null;
        }
        UIManager.Instance.GamePlayPanel.ShowReSpawnTime("");
        
        Respawn();
        ShowPlayerModel();
    }

    private void Respawn()
    {
        Transform spawnPoint = PlayerSpawnManager.Instance.GetSpawnPoint();
        if(Object.HasInputAuthority) _kcc.TeleportRPC(spawnPoint.position, spawnPoint.rotation.eulerAngles.x, spawnPoint.rotation.eulerAngles.y);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcDie()
    {
        Die();
    }

    public bool IsDead() {
        return _isDead;
    }

    public void Reset() {
        _isDead = false;
        _isDisable = false;
        _kcc.enabled = true;
        _kccCollider.SetActive(true);
        _playerHealth.ResetHealth();
        _playerAnimation.Reset();
    }
    #endregion

    public void HidePlayerModel()
    {
        _model.SetActive(false);
        _kccCollider.layer = LayerMask.NameToLayer("IgnorePlayerCollision");
         Reset();
    }

    public void ShowPlayerModel()
    {
        _model.SetActive(true);
        _kccCollider.layer = LayerMask.NameToLayer("Default");
    }

    public KCC GetKCC()
    {
        return _kcc;
    }

    public GameObject GetModel()
    {
        return _model;
    }
}