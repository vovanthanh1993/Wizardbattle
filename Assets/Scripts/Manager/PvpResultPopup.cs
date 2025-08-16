using UnityEngine;

public class PvpResultPopup : MonoBehaviour
{
    public static PvpResultPopup Instance { get; private set; }

    [SerializeField] private GameObject _background;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _losePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        _background.gameObject.SetActive(false);
        _winPanel.SetActive(false);
        _losePanel.SetActive(false);
    }

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowResultPopup(bool isWin) {
        _background.SetActive(true);
        _winPanel.SetActive(isWin);
        _losePanel.SetActive(!isWin);
    }

    public void ShowResultPopupForPlayer() {
        _background.SetActive(true);
        var runner = NetworkRunnerHandler.Instance?.Runner;
        if (runner != null)
        {
            var localPlayer = runner.GetPlayerObject(runner.LocalPlayer);
            if (localPlayer != null)
            {
                var localPlayerStatus = localPlayer.GetComponent<PlayerStatus>();
                if (localPlayerStatus != null)
                {
                    if (localPlayerStatus.IsWin)
                    {
                        _winPanel.SetActive(true);
                        _losePanel.SetActive(false);
                        return;
                    }
                }
            }
        }
        _winPanel.SetActive(false);
        _losePanel.SetActive(true);
    }
}
