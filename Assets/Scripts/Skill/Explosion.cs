using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private float _lifetime = 2f;
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
            GamePoolManager.Instance.ReturnExplosion(this);
        }
        else
        {
            // Fallback if pool manager is not available
            Destroy(gameObject);
        }
    }
}
