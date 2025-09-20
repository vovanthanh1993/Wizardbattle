using UnityEngine;

public class XPItemE : MonoBehaviour
{
    [Header("XP Settings")]
    [SerializeField] private int _xpAmount = 50;
    [SerializeField] private int _xpVariation = 5;

    private bool _isPickedUp = false;
    private int _actualXpAmount;

    private void OnEnable()
    {
        _isPickedUp = false;
        _actualXpAmount = _xpAmount + Random.Range(-_xpVariation, _xpVariation + 1);
        _actualXpAmount = Mathf.Max(1, _actualXpAmount);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isPickedUp) return;

        var playerStatus = other.GetComponentInParent<PlayerStatus>();
        if (playerStatus != null)
        {
            _isPickedUp = true;
            playerStatus.AddXP(_actualXpAmount);
            GamePoolManager.Instance.ReturnXpItem(this);
        }
    }
}
