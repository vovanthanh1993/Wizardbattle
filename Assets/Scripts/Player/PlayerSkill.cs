using UnityEngine;
using System.Collections;
using Fusion;
public class PlayerSkill : NetworkBehaviour
{
    [SerializeField] private ParticleSystem _powerUpParticleSystem;

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcPlayPowerUp(float time)
    {
        StartCoroutine(PlayPowerUpParticleEffect(time));
    }

    private IEnumerator PlayPowerUpParticleEffect(float time)
    {
        _powerUpParticleSystem.gameObject.SetActive(true);
        _powerUpParticleSystem.Play();
        
        yield return new WaitForSeconds(time);

        _powerUpParticleSystem.gameObject.SetActive(false);
    }
}
