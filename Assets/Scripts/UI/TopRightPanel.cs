using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopRightPanel : MonoBehaviour
{
    
    [SerializeField] private TMP_Text _rubyText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _foodText;

    void Start()
    {
        InitData();
    }

    public void InitData() {
        _rubyText.text = FirebaseDataManager.Instance.GetCurrentUserRuby().ToString();
        _goldText.text = FirebaseDataManager.Instance.GetCurrentUserGold().ToString();
        _foodText.text = FirebaseDataManager.Instance.GetCurrentUserFood().ToString();
    }
}
