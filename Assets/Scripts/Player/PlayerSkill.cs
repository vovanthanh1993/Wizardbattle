using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Addons.KCC;
public class PlayerSkill : NetworkBehaviour
{
    [Header("FireBall Skill")]
    [SerializeField] private float _fireRate = 1f;
    [SerializeField] private GameObject _fireBallPrefab;
    [SerializeField] private Transform _firePoint;

    [Header("Heal Skill")]
    [SerializeField] private float _healRate = 60f;
    [SerializeField] private int _healAmount = 300;
    
    [Header("Run Skill")]
    [SerializeField] private float _runRate = 40f;
    [SerializeField] private float _runDuration = 3f;
    [SerializeField] private float _runAmount = 5f;
    [SerializeField] private ParticleSystem _runEffectParticleSystem;

    [Header("Stealth Skill")]
    [SerializeField] private Material _hideMaterial;
    [SerializeField] private int  _stealthRate = 30;
    [SerializeField] private int  _stealthDuration = 5;

    private float _nextFireTime;
    private float _nextRunTime;
    private float _nextHealTime;
    private float _nextStealthTime;
    private Material[] _originalMaterials;
     

    private PlayerController _playerController;
    private PlayerStatus _playerStatus;
    private PlayerHealth _playerHealth;
    private PlayerAnimation _playerAnimation;

    public override void Spawned()
    {
        _playerController = GetComponent<PlayerController>();
        _playerStatus = GetComponent<PlayerStatus>();
        _playerHealth = GetComponent<PlayerHealth>();
        _playerAnimation = GetComponent<PlayerAnimation>();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcPlayRunEffect(float time)
    {
        StartCoroutine(PlayRunEffectParticleEffect(time));
        if (HasInputAuthority) AudioManager.Instance.PlaySkillRunSound();
    }

    private IEnumerator PlayRunEffectParticleEffect(float time)
    {
        _runEffectParticleSystem.gameObject.SetActive(true);
        _runEffectParticleSystem.Play();
        
        yield return new WaitForSeconds(time);

        _runEffectParticleSystem.gameObject.SetActive(false);
    }

    public void SetHideMaterial()
    {
        SaveOriginalMaterials();
        if (_hideMaterial == null) return;
        
        Renderer[] renderers = _playerController.GetModel().GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = _hideMaterial;
            }
            
            renderer.materials = materials;
        }
    }

    public void SaveOriginalMaterials()
    {
        if (_playerController.GetModel() == null) return;
        
        Renderer[] renderers = _playerController.GetModel().GetComponentsInChildren<Renderer>();
        List<Material> allMaterials = new List<Material>();
        
        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.materials)
            {
                allMaterials.Add(mat);
            }
        }
        
        _originalMaterials = allMaterials.ToArray();
    }

    public void RestoreOriginalMaterials()
    {
        if (_playerController.GetModel() == null || _originalMaterials == null) return;
        
        Renderer[] renderers = _playerController.GetModel().GetComponentsInChildren<Renderer>();
        int materialIndex = 0;
        
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length && materialIndex < _originalMaterials.Length; i++)
            {
                materials[i] = _originalMaterials[materialIndex];
                materialIndex++;
            }
            renderer.materials = materials;
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcStealth()
    {
        if (Runner.SimulationTime < _nextStealthTime) return;
        
        _nextStealthTime = Runner.SimulationTime + _stealthRate;
        if (Object.HasInputAuthority) UIManager.Instance.GamePlayPanel.StartStealthCooldown(_stealthRate);
        StartCoroutine(StealthCoroutine());
        
        if (AudioManager.Instance != null)
        {
            //AudioManager.Instance.PlayHealSound();
        }
    }

    private IEnumerator StealthCoroutine()
    {
        if (Object.HasInputAuthority) SetHideMaterial();
        else _playerController.GetModel().SetActive(false);
        yield return new WaitForSeconds(_stealthDuration);
        
        if (Object.HasInputAuthority) RestoreOriginalMaterials();
        else _playerController.GetModel().SetActive(true);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcRun()
    {
        if (Runner.SimulationTime < _nextRunTime) return;
        
        StartCoroutine(TemporarySpeedBoost());
        RpcPlayRunEffect(_runDuration);
        _nextRunTime = Runner.SimulationTime + _runRate;
        
        if (Object.HasInputAuthority)
        {
            UIManager.Instance.GamePlayPanel.StartRunCooldown(_runRate);
        }
    }

    private IEnumerator TemporarySpeedBoost()
    {
        var processors = _playerController.GetKCC().LocalProcessors;
        foreach (var processor in processors)
        {
            if (processor is EnvironmentProcessor envProcessor)
            {
                envProcessor.KinematicSpeed = _playerStatus.Speed + _runAmount;
                break;
            }
        }

        yield return new WaitForSeconds(_runDuration);
        foreach (var processor in processors)
        {
            if (processor is EnvironmentProcessor envProcessor)
            {
                envProcessor.KinematicSpeed = _playerStatus.Speed;
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcHeal()
    {
        if (Runner.SimulationTime < _nextHealTime) return;
        _playerHealth.Heal(_healAmount);
        _nextHealTime = Runner.SimulationTime + _healRate;
        if (Object.HasInputAuthority) UIManager.Instance.GamePlayPanel.StartHealingCooldown(_healRate);
    }

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
            return _firePoint.position + cameraDirection * 0.8f;
        }
        
        // Fallback to firePoint position
        return _firePoint.position + _firePoint.forward * 0.8f;
    }
}
