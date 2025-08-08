using Fusion;
using UnityEngine;

public class HealthItem : NetworkBehaviour
{
    [SerializeField] private int _healAmount = 20;
    [Networked] private bool _isPickedUp { get; set; }

    public override void Spawned()
    {
        _isPickedUp = false;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickedUp) return;

        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null && Object.HasStateAuthority)
        {
            _isPickedUp = true;
            playerHealth.Heal(_healAmount);
            RpcHideItem();
            ItemSpawner.Instance.OnHealthItemPickedUp();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcHideItem()
    {
        gameObject.SetActive(false);
    }
}
