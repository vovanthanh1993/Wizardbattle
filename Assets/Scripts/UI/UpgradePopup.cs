using UnityEngine;
using TMPro;
public enum UpgradeType
{
    Health,
    Damage,
    Ammor
    
}
public class UpgradePopup : MonoBehaviour
{
    [Header("Number")]
    [SerializeField] private TMP_Text _numberText;

    [Header("Gold Shop - 3 Levels")]
    [SerializeField] private int _healthUpgrade = 10;
    [SerializeField] private int _damageUpgrade = 10;

    [SerializeField] private int _ammorUpgrade = 20;
    
    [SerializeField] private int _goldCost = 200;

    public void UpdateNumber()
    { 
        if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserGold().ToString();
    }
    
    private void OnEnable()
    {
        UpdateNumber();
    }

    public void UpgradeHealth()
    {
        Upgrade(_goldCost, UpgradeType.Health, _healthUpgrade);
    }
    
    public void UpgradeDamage()
    {
        Upgrade(_goldCost, UpgradeType.Damage, _damageUpgrade);
    }
    
    public void UpgradeAmmor()
    {
        Upgrade(_goldCost, UpgradeType.Ammor, _ammorUpgrade);
    }

    public async void Upgrade(int gold, UpgradeType upgradeType, int upgradeAmount)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserGold() >= gold)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            bool isSuccess = await FirebaseDataManager.Instance.Upgrade(gold, upgradeType, upgradeAmount);
            if (isSuccess)
            {
                Debug.Log($"Upgrade {upgradeType} Success");
                UIManager.Instance.ShowNoticePopup($"Upgrade {upgradeAmount} {upgradeType} Success!");
                UIManager.Instance.TopRightPanel.InitData();
                UIManager.Instance.TopLeftPanel.InitData();
                UpdateNumber();
                UIManager.Instance.ShowLoadingPanel(false);
            }
            else
            {
                Debug.Log($"Upgrade {upgradeType} Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup($"Upgrade {upgradeType} Failed! Please try again.");
            }
        } else UIManager.Instance.ShowNoticePopup("You don't have enough gold!");
    }
}
