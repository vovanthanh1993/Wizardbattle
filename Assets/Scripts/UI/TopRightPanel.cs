using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TopRightPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinText;
    [SerializeField] private TMP_Text _diamondText;
    [SerializeField] private TMP_Text _lifeText;

    void Start()
    {
        InitData();
    }

    public void InitData() {
        _coinText.text = FirebaseDataManager.Instance.GetCurrentUserCoin().ToString();
        _diamondText.text = FirebaseDataManager.Instance.GetCurrentUserDiamond().ToString();
        _lifeText.text = FirebaseDataManager.Instance.GetCurrentUserLife().ToString();
    }
}
