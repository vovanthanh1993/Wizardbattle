using UnityEngine;

public enum ExplosionType
{
    Player,
    DragonForest
}

public class Explosion : MonoBehaviour
{
    [SerializeField] private ExplosionType _explosionType;
    [SerializeField] private float _lifetime = 1f;
    private float _timer;
    private bool _isActive = false;

    void Start()
    {
        ResetState();
    }

    void Update()
    {
        if (!_isActive) return;
        
        _timer += Time.deltaTime;
        
        if (_timer >= _lifetime)
        {
            ReturnToPool();
        }
    }

    public void ResetState()
    {
        _timer = 0f;
        _isActive = true;
    }

    private void ReturnToPool()
    {
        _isActive = false;
        
        if (GamePoolManager.Instance != null)
        {
            if (_explosionType == ExplosionType.Player)
            {
                GamePoolManager.Instance.ReturnExplosion(this);
            }
            else if (_explosionType == ExplosionType.DragonForest)
            {
                GamePoolManager.Instance.ReturnDragonForestExplosion(this);
            }
        }
        else
        {
            // Fallback if pool manager is not available
            Destroy(gameObject);
        }
    }
}
