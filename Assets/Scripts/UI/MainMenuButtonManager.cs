using UnityEngine;

public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject _singlePlayerMap;
    [SerializeField] private GameObject _multiplayerMap;

    [SerializeField] private int _foodValue = 1;

    private void OnEnable() {
        UIManager.Instance.TopLeftPanel.InitData();
    }

    public void ShowSinglePlayerMap()
    {
        
        if(FirebaseDataManager.Instance.GetCurrentUserFood() >= _foodValue)
        {
            gameObject.SetActive(false);
            _singlePlayerMap.SetActive(true);
            AudioManager.Instance.PlayGameModeSelectSound();
        } else {
            UIManager.Instance.ShowNoticePopup("You need food to start the journey!");
            AudioManager.Instance.PlayNotEnoughSound();
        }
    }

    public void ShowMultiplayerMap()
    {
        if(FirebaseDataManager.Instance.GetCurrentUserFood() >= _foodValue)
        {
            gameObject.SetActive(false);
            _multiplayerMap.SetActive(true);
            AudioManager.Instance.PlayGameModeSelectSound();
        }
        else
        {
            UIManager.Instance.ShowNoticePopup("You need food to start the journey!");
            AudioManager.Instance.PlayNotEnoughSound();
        }
    }
}
