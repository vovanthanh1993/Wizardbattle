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

    [Header("Pool Parent")]
    [SerializeField] private Transform _poolParent;

    private ObjectPool<FireBall> _fireBallPool;
    private ObjectPool<Explosion> _explosionPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
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

    public void ReturnAll()
    {
        ReturnAllFireBalls();
        ReturnAllExplosions();
    }
} 