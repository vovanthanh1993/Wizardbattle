using UnityEngine;
using System.Collections.Generic;

public class DragonForestFireBall : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private float _explosionDamage = 100f;

    [SerializeField] private float _fireballSpeed = 25f;
    [SerializeField] private float _fireballLifetime = 3f;

    private Vector3 _direction;
    private float _timer;

    public void Init(Vector3 direction)
    {
        _direction = direction;
        _timer = 0f;
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += _direction * _fireballSpeed * Time.deltaTime;
        _timer += Time.deltaTime;
        if (_timer > _fireballLifetime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {   
        // Don't explode when colliding with Item, Enemy
        if (other.CompareTag("Item") || other.CompareTag("Enemy")) 
        {
            return;
        }
        
        SpawnExplosionAndDamage();
        ReturnToPool();
    }

    private void SpawnExplosionAndDamage()
    {
        Vector3 explosionPosition = transform.position;
        
        // Spawn explosion effect from pool
        Explosion explosion = GamePoolManager.Instance.GetDragonForestExplosion();
        
        if (explosion != null)
        {
            explosion.transform.position = explosionPosition;
            explosion.transform.rotation = Quaternion.identity;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosionSoundAtPosition(explosionPosition);
            }
        }

        // Find all players within explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, _explosionRadius);
        HashSet<PlayerHealth> damagedPlayers = new HashSet<PlayerHealth>();

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.GetComponentInParent<PlayerHealth>() != null)
            {
                var playerHealth = hitCollider.GetComponentInParent<PlayerHealth>();
                if (playerHealth != null && !damagedPlayers.Contains(playerHealth))
                {
                    // Calculate damage based on distance to closest point of collider
                    float damage = CalculateDamageByDistance(explosionPosition, hitCollider, _explosionDamage);
                    Debug.Log("DragonForestFireball Damage: " + damage);
                    
                    // Deal damage directly (offline mode)
                    if(damage > 0) 
                    {
                        playerHealth.TakeDamage((int)damage, null);
                    }
                    damagedPlayers.Add(playerHealth);
                }
            }
        }
    }

    private float CalculateDamageByDistance(Vector3 explosionPos, Collider hitCollider, float baseDamage)
    {
        // Tính khoảng cách từ explosion đến điểm gần nhất của collider
        Vector3 closestPoint = hitCollider.ClosestPoint(explosionPos);
        float distance = Vector3.Distance(explosionPos, closestPoint);
        
        if (distance <= 0) return baseDamage;
        if (distance >= _explosionRadius) return 0;
        
        // Damage decreases with distance (linear falloff)
        float damageMultiplier = 1f - (distance / _explosionRadius);
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    private void ReturnToPool()
    {
        if (GamePoolManager.Instance != null)
        {
            GamePoolManager.Instance.ReturnDragonForestFireball(this);
        }
        else
        {
            // Fallback if pool manager is not available
            Destroy(gameObject);
        }
    }
}
