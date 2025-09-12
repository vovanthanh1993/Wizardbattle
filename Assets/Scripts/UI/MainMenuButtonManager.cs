using UnityEngine;

public class MainMenuButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject _singlePlayerMap;
    [SerializeField] private GameObject _multiplayerMap;

    [SerializeField] private int _foodValue = 1;

    public void ShowSinglePlayerMap()
    {
        if(FirebaseDataManager.Instance.GetCurrentUserFood() >= _foodValue)
        {
            gameObject.SetActive(false);
            _singlePlayerMap.SetActive(true);
        } else {
            UIManager.Instance.ShowNoticePopup("You need food to start the journey!");
        }
    }

    public void ShowMultiplayerMap()
    {
        if(FirebaseDataManager.Instance.GetCurrentUserFood() >= _foodValue)
        {
            gameObject.SetActive(false);
            _multiplayerMap.SetActive(true);
        }
        else
        {
            UIManager.Instance.ShowNoticePopup("You need food to start the journey!");
        }
    }
}
