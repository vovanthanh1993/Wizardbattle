using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Fusion;
public class PlayerSkill : NetworkBehaviour
{
    [SerializeField] private ParticleSystem _powerUpParticleSystem;
    [SerializeField] private Material _hideMaterial;
    private Material[] _originalMaterials;
    [SerializeField] private int  _stealthRate = 30;
    [SerializeField] private int  _stealthDuration = 5;
    private float _nextStealthTime;

    private PlayerController _playerController;

    public override void Spawned()
    {
        _playerController = GetComponent<PlayerController>();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcPlayPowerUp(float time)
    {
        StartCoroutine(PlayPowerUpParticleEffect(time));
        if (HasInputAuthority) AudioManager.Instance.PlaySkillRunSound();
    }

    private IEnumerator PlayPowerUpParticleEffect(float time)
    {
        _powerUpParticleSystem.gameObject.SetActive(true);
        _powerUpParticleSystem.Play();
        
        yield return new WaitForSeconds(time);

        _powerUpParticleSystem.gameObject.SetActive(false);
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
}
