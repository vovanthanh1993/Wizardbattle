using UnityEngine;
using Fusion;

public class XpItem : NetworkBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int _xpAmount = 50;
    [SerializeField] private int _xpVariation = 5;
    
    [Networked] private bool _isPickedUp { get; set; }
    [Networked] private int _actualXpAmount { get; set; }

    public override void Spawned()
    {
        _isPickedUp = false;
        if (Object.HasStateAuthority)
        {
            _actualXpAmount = _xpAmount + Random.Range(-_xpVariation, _xpVariation + 1);
            _actualXpAmount = Mathf.Max(1, _actualXpAmount);
        }
        
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickedUp) return;

        var playerStatus = other.GetComponentInParent<PlayerStatus>();
        if (playerStatus != null && Object.HasStateAuthority)
        {
            _isPickedUp = true;
            playerStatus.AddXP(_actualXpAmount);
            RpcHideItem();
            ItemSpawner.Instance.OnXpItemPickedUp();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RpcHideItem()
    {
        gameObject.SetActive(false);
    }
    
    public int GetXpAmount()
    {
        return _actualXpAmount;
    }
}
