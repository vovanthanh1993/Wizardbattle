using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GamePoolManager : MonoBehaviour
{
    public static GamePoolManager Instance { get; private set; }

    [Header("FireBall Pool Settings")]
    [SerializeField] private GameObject _fireBallPrefab;
    [SerializeField] private int _fireBallPoolSize = 20;

    [Header("Explosion Pool Settings")]
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private int _explosionPoolSize = 10;

    [Header("XP Item Pool Settings")]
    [SerializeField] private GameObject _xpItemPrefab;
    [SerializeField] private int _xpItemPoolSize = 10;

    [Header("XP Item Red Pool Settings")]
    [SerializeField] private GameObject _xpItemRedPrefab;
    [SerializeField] private int _xpItemRedPoolSize = 10;

    [Header("Health Pool Settings")]
    [SerializeField] private GameObject _healthPrefab;
    [SerializeField] private int _healthPoolSize = 10;

    [Header("Dragon Forest Fireball Pool Settings")]
    [SerializeField] private GameObject _dragonForestFireBallPrefab;
    [SerializeField] private int _dragonForestFireballPoolSize = 10;

    [Header("Dragon Forest Explosion Pool Settings")]
    [SerializeField] private GameObject _dragonForestExplosionPrefab;
    [SerializeField] private int _dragonForestExplosionPoolSize = 10;

    [Header("Pool Parent")]
    [SerializeField] private Transform _poolParent;

    private ObjectPool<FireBall> _fireBallPool;
    private ObjectPool<Explosion> _explosionPool;
    private ObjectPool<XPItemE> _xpItemPool;
    private ObjectPool<XPItemE> _xpItemRedPool;
    private ObjectPool<Health> _healthPool;
    private ObjectPool<DragonForestFireBall> _dragonForestFireBallPool;
    private ObjectPool<Explosion> _dragonForestExplosionPool; 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        InitializePools();
    }

    private void InitializePools()
    {
        // Initialize FireBall Pool
        if (_fireBallPrefab != null)
        {
            var fireBall = _fireBallPrefab.GetComponent<FireBall>();
            if (fireBall != null)
            {
                _fireBallPool = new ObjectPool<FireBall>(fireBall, _fireBallPoolSize, _poolParent);
            }
        }

        // Initialize Explosion Pool
        if (_explosionPrefab != null)
        {
            var explosion = _explosionPrefab.GetComponent<Explosion>();
            if (explosion != null)
            {
                _explosionPool = new ObjectPool<Explosion>(explosion, _explosionPoolSize, _poolParent);
            }
        }

        // Initialize XP Item Pool
        if (_xpItemPrefab != null)
        {
            var xpItem = _xpItemPrefab.GetComponent<XPItemE>();
            if (xpItem != null)
            {
                _xpItemPool = new ObjectPool<XPItemE>(xpItem, _xpItemPoolSize, _poolParent);
            }
        }

        // Initialize XP Item Red Pool
        if (_xpItemRedPrefab != null)
        {
            var xpItemRed = _xpItemRedPrefab.GetComponent<XPItemE>();
            if (xpItemRed != null)
            {
                _xpItemRedPool = new ObjectPool<XPItemE>(xpItemRed, _xpItemRedPoolSize, _poolParent);
            }
        }

        // Initialize Health Pool
        if (_healthPrefab != null)
        {
            var health = _healthPrefab.GetComponent<Health>();
            if (health != null)
            {
                _healthPool = new ObjectPool<Health>(health, _healthPoolSize, _poolParent);
            }
        }

        // Initialize Dragon Forest Fireball Pool
        if (_dragonForestFireBallPrefab != null)
        {
            var dragonForestFireBall = _dragonForestFireBallPrefab.GetComponent<DragonForestFireBall>();
            if (dragonForestFireBall != null)
            {
                _dragonForestFireBallPool = new ObjectPool<DragonForestFireBall>(dragonForestFireBall, _dragonForestFireballPoolSize, _poolParent);
            }
        }

        // Initialize Dragon Forest Explosion Pool
        if (_dragonForestExplosionPrefab != null)
        {
            var dragonForestExplosion = _dragonForestExplosionPrefab.GetComponent<Explosion>();
            if (dragonForestExplosion != null)
            {
                _dragonForestExplosionPool = new ObjectPool<Explosion>(dragonForestExplosion, _dragonForestExplosionPoolSize, _poolParent);
            }
        }
    }

    public void ReturnXpItem(XPItemE xpItem)
    {
        if (_xpItemPool != null && xpItem != null)
        {
            _xpItemPool.Return(xpItem);
        }
    }

    public void ReturnXpItemRed(XPItemE xpItemRed)
    {
        if (_xpItemRedPool != null && xpItemRed != null)
        {
            _xpItemRedPool.Return(xpItemRed);
        }
    }

    public XPItemE GetXpItemRed()
    {
        if (_xpItemRedPool == null)
        {
            return null;
        }
        return _xpItemRedPool.Get();
    }

    public XPItemE GetXpItem()
    {
        if (_xpItemPool == null)
        {
            return null;
        }
        return _xpItemPool.Get();
    }

    public Health GetHealth()
    {
        if (_healthPool == null)
        {
            return null;
        }
        return _healthPool.Get();
    }

    public void ReturnHealth(Health health)
    {
        if (_healthPool != null && health != null)
        {
            _healthPool.Return(health);
        }
    }

    // Dragon Forest Fireball Pool Methods
    public DragonForestFireBall GetDragonForestFireball()
    {
        if (_dragonForestFireBallPool == null)
        {
            return null;
        }
        return _dragonForestFireBallPool.Get();
    }

    public void ReturnDragonForestFireball(DragonForestFireBall dragonForestFireBall)
    {
        if (_dragonForestFireBallPool != null && dragonForestFireBall != null)
        {
            _dragonForestFireBallPool.Return(dragonForestFireBall);
        }
    }

    // Dragon Forest Explosion Pool Methods
    public Explosion GetDragonForestExplosion()
    {
        if (_dragonForestExplosionPool == null)
        {
            return null;
        }
        return _dragonForestExplosionPool.Get();
    }

    public void ReturnDragonForestExplosion(Explosion dragonForestExplosion)
    {
        if (_dragonForestExplosionPool != null && dragonForestExplosion != null)
        {
            _dragonForestExplosionPool.Return(dragonForestExplosion);
        }
    }

    // FireBall Pool Methods
    public FireBall GetFireBall()
    {
        if (_fireBallPool == null)
        {
            return null;
        }

        return _fireBallPool.Get();
    }
    public void ReturnFireBall(FireBall fireBall)
    {
        if (_fireBallPool != null && fireBall != null)
        {
            _fireBallPool.Return(fireBall);
        }
    }

    // Explosion Pool Methods
    public Explosion GetExplosion()
    {
        if (_explosionPool == null)
        {
            return null;
        }

        return _explosionPool.Get();
    }

    public void ReturnExplosion(Explosion explosion)
    {
        if (_explosionPool != null && explosion != null)
        {
            _explosionPool.Return(explosion);
        }
    }

    // Return All Methods
    public void ReturnAllFireBalls()
    {
        if (_fireBallPool != null)
        {
            _fireBallPool.ReturnAll();
        }
    }

    public void ReturnAllExplosions()
    {
        if (_explosionPool != null)
        {
            _explosionPool.ReturnAll();
        }
    }

    public void ReturnAllXpItems()
    {
        if (_xpItemPool != null)
        {
            _xpItemPool.ReturnAll();
        }
    }

    public void ReturnAllXpItemRed()
    {
        if (_xpItemRedPool != null)
        {
            _xpItemRedPool.ReturnAll();
        }
    }

    public void ReturnAllHealth()
    {
        if (_healthPool != null)
        {
            _healthPool.ReturnAll();
        }
    }

    public void ReturnAllDragonForestFireBalls()
    {
        if (_dragonForestFireBallPool != null)
        {
            _dragonForestFireBallPool.ReturnAll();
        }
    }

    public void ReturnAllDragonForestExplosions()
    {
        if (_dragonForestExplosionPool != null)
        {
            _dragonForestExplosionPool.ReturnAll();
        }
    }

    public void ReturnAll()
    {
        ReturnAllFireBalls();
        ReturnAllExplosions();
        ReturnAllXpItems();
        ReturnAllXpItemRed();
        ReturnAllHealth();
        ReturnAllDragonForestFireBalls();
        ReturnAllDragonForestExplosions();
    }
} 