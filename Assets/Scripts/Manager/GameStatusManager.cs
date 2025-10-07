using UnityEngine;

public enum GameStatus {
    INMENU,
    PLAYING,
    ENDGAME
}
public class GameStatusManager : MonoBehaviour
{
    public static GameStatusManager Instance { get; private set; }

    private GameStatus _gameStatus = GameStatus.INMENU;
    
    [Header("Game Time")]
    [SerializeField] private float _gameStartTime;
    [SerializeField] private float _gameEndTime;
    [SerializeField] private float _totalGameTime;

    [SerializeField] private Transform _bossSpawnPoint;
    [SerializeField] private float _bossSpawnTime = 300f;
    [SerializeField] private bool _bossSpawned = false;
    [SerializeField] private bool _bossWarningPlayed = false;

    [SerializeField] private GameObject _dragonForestPrefab;

    [SerializeField] private bool _isPause = false;

    private void Awake()
    {
    if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start() {
        SetGameStatus(GameStatus.PLAYING);
    }

    private void Update() {
        if (_gameStatus == GameStatus.PLAYING) {
            UIManager.Instance.GamePlayPanel.SetTimeText(GameCommonUtils.GetGameTimeString(GetCurrentGameTime()));
            
            // Check boss spawn after 5 minutes
            CheckBossSpawn();
        }
    }

    public void SetIsPause(bool isPause)
    {
        _isPause = isPause;
    }

    public bool GetIsPause()
    {
        return _isPause;
    }

    public void SetGameStatus(GameStatus gameStatus)
    {
        _gameStatus = gameStatus;
        
        // Manage game time
        if (gameStatus == GameStatus.PLAYING)
        {
            StartGameTime();
        }
        else if (gameStatus == GameStatus.ENDGAME)
        {
            EndGameTime();
        }
    }

    public GameStatus GetGameStatus()
    {
        return _gameStatus;
    }
    
    // Start game time tracking
    private void StartGameTime()
    {
        _gameStartTime = Time.time;
        _totalGameTime = 0f;
        Debug.Log("Game started at: " + _gameStartTime);
    }
    
    // End game time tracking
    private void EndGameTime()
    {
        _gameEndTime = Time.time;
        _totalGameTime = _gameEndTime - _gameStartTime;
        Debug.Log("Game ended. Total time: " + _totalGameTime + " seconds");
    }
    
    // Get current game time (if playing)
    public float GetCurrentGameTime()
    {
        if (_gameStatus == GameStatus.PLAYING)
        {
            return Time.time - _gameStartTime;
        }
        return _totalGameTime;
    }
    
    // Get total game time played
    public float GetTotalGameTime()
    {
        return _totalGameTime;
    }
    
    
    
    // Reset game time
    public void ResetGameTime()
    {
        _gameStartTime = 0f;
        _gameEndTime = 0f;
        _totalGameTime = 0f;
        _bossSpawned = false; // Reset boss spawn status
        _bossWarningPlayed = false; // Reset boss warning status
    }
    
    // Check and spawn boss
    private void CheckBossSpawn()
    {
        if (!_bossSpawned && GetCurrentGameTime() >= _bossSpawnTime)
        {
            SpawnBoss();
        } else if(!_bossSpawned && !_bossWarningPlayed && GetCurrentGameTime() >= _bossSpawnTime - 3)
        {
            _bossWarningPlayed = true;
            UIManager.Instance.GamePlayPanel.SetWarningText("Alert! Boss is comming!");
            UIManager.Instance.GamePlayPanel.ShowIntroBossHealth(3);
        }
    }
    
    // Spawn boss
    private void SpawnBoss()
    {
        if (_bossSpawnPoint != null && _dragonForestPrefab != null)
        {
            _bossSpawned = true;
            Instantiate(_dragonForestPrefab, _bossSpawnPoint.position, _bossSpawnPoint.rotation);
        }
    }
}
