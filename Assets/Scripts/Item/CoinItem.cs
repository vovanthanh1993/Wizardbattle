using UnityEngine;
using Fusion;
public class CoinItem : NetworkBehaviour
{
    [SerializeField] private int _coinAmount = 20;
    [Networked] private bool _isPickedUp { get; set; }

    public override void Spawned()
    {
        _isPickedUp = false;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickedUp) return;

        var playerStatus = other.GetComponentInParent<PlayerStatus>();
        if (playerStatus != null && Object.HasStateAuthority)
        {
            _isPickedUp = true;
            playerStatus.AddCoin(_coinAmount);
            RpcHideItem();
            ItemSpawner.Instance.OnCoinItemPickedUp();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcHideItem()
    {
        gameObject.SetActive(false);
    }
}
