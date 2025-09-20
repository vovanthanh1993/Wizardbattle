using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int _healAmount = 20;

    private void OnTriggerEnter(Collider other)
    {
        var playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.Heal(_healAmount);
            GamePoolManager.Instance.ReturnHealth(this);
        }
    }
}
