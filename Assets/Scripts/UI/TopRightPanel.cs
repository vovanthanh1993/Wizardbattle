using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopRightPanel : MonoBehaviour
{
    
    [SerializeField] private TMP_Text _rubyText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _keyText;

    void Start()
    {
        InitData();
    }

    public void InitData() {
        _rubyText.text = FirebaseDataManager.Instance.GetCurrentUserRuby().ToString();
        _goldText.text = FirebaseDataManager.Instance.GetCurrentUserGold().ToString();
        _keyText.text = FirebaseDataManager.Instance.GetCurrentUserKey().ToString();
    }
}
