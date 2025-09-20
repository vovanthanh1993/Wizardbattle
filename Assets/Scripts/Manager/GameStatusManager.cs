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

    private void Awake()
    {
    if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGameStatus(GameStatus gameStatus)
    {
        _gameStatus = gameStatus;
    }

    public GameStatus GetGameStatus()
    {
        return _gameStatus;
    }
}
