using UnityEngine;
using TMPro;

public class TopLeftPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _damageText;
    [SerializeField] private TMP_Text _speedText;
    [SerializeField] private TMP_Text _healthText;

    void Start()
    {
        InitData();
    }

    public void InitData() {
        _playerNameText.text = FirebaseDataManager.Instance.GetCurrentUserDisplayName();
        _levelText.text = FirebaseDataManager.Instance.GetCurrentUserLevel().ToString();
        _damageText.text = FirebaseDataManager.Instance.GetCurrentUserDamage().ToString();
        _speedText.text = FirebaseDataManager.Instance.GetCurrentUserSpeed().ToString();
        _healthText.text = FirebaseDataManager.Instance.GetCurrentPlayerData().health.ToString();
    }
}
