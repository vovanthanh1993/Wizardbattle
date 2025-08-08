using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class FireBall : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private float _explosionRadius = 3f;
    [SerializeField] private int _explosionDamage = 20;

    private Vector3 _direction;
    private float _speed;
    private float _lifetime;
    private float _timer;
    private NetworkObject _shooter;

    public void Init(Vector3 direction, float speed, float lifetime, NetworkObject shooter = null)
    {
        _direction = direction;
        _speed = speed;
        _lifetime = lifetime;
        _timer = 0f;
        _shooter = shooter;
        gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
        _timer += Time.deltaTime;
        if (_timer > _lifetime)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {   
        var hitRoot = other.GetComponentInParent<NetworkObject>();
        
        // Only don't explode when colliding with the shooter
        if (_shooter != null && hitRoot == _shooter) 
        {
            return;
        }
        
        // Don't explode when colliding with Item
        if (other.CompareTag("Item")) 
        {
            return;
        }
        
        // Explode when colliding with anyone/anything else (except Item)
        SpawnExplosionAndDamage();
        ReturnToPool();
    }

    private void SpawnExplosionAndDamage()
    {
        Vector3 explosionPosition = transform.position;
        
        // Spawn explosion effect from pool
        Explosion explosion = null;
        if (GamePoolManager.Instance != null)
        {
            explosion = GamePoolManager.Instance.GetExplosion();
        }
        else
        {
            // Fallback if pool manager is not available
            if (_explosionPrefab != null)
            {
                var explosionObj = Instantiate(_explosionPrefab, explosionPosition, Quaternion.identity);
                explosion = explosionObj.GetComponent<Explosion>();
            }
        }
        
        if (explosion != null)
        {
            explosion.transform.position = explosionPosition;
            explosion.transform.rotation = Quaternion.identity;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayExplosionSound();
            }
        }

        // Find all players within explosion radius
        Collider[] hitColliders = Physics.OverlapSphere(explosionPosition, _explosionRadius);
        HashSet<NetworkObject> damagedPlayers = new HashSet<NetworkObject>();

        foreach (var hitCollider in hitColliders)
        {
            var playerRoot = hitCollider.GetComponentInParent<NetworkObject>();
            
            if (playerRoot == null || playerRoot == _shooter) continue;
            
            var playerController = playerRoot.GetComponent<PlayerController>();
            if (playerController != null && !damagedPlayers.Contains(playerRoot))
            {
                // Calculate damage based on distance
                float distance = Vector3.Distance(explosionPosition, playerRoot.transform.position);
                int damage = CalculateDamageByDistance(distance);
                
                // Send RPC to deal damage
                RpcRequestDamage(playerRoot, damage, _shooter);
                damagedPlayers.Add(playerRoot);
            }
        }
    }

    private int CalculateDamageByDistance(float distance)
    {
        if (distance <= 0) return _explosionDamage;
        if (distance >= _explosionRadius) return 0;
        
        // Damage decreases with distance (linear falloff)
        float damageMultiplier = 1f - (distance / _explosionRadius);
        return Mathf.RoundToInt(_explosionDamage * damageMultiplier);
    }

    private void ReturnToPool()
    {
        if (GamePoolManager.Instance != null)
        {
            GamePoolManager.Instance.ReturnFireBall(this);
        }
        else
        {
            // Fallback if pool manager is not available
            Destroy(gameObject);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcRequestDamage(NetworkObject targetPlayer, int damage, NetworkObject shooter)
    {
        var playerHealth = targetPlayer.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage, shooter);
        }
    }

    // Gizmos to debug explosion radius
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
} 