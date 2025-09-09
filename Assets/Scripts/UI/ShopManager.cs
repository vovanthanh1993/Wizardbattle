using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [Header("Gold Shop - 3 Levels")]
    [SerializeField] private int _goldRubyCost1 = 50;
    [SerializeField] private int _goldAmount1 = 200;
    
    [SerializeField] private int _goldRubyCost2 = 120;
    [SerializeField] private int _goldAmount2 = 500;
    
    [SerializeField] private int _goldRubyCost3 = 200;
    [SerializeField] private int _goldAmount3 = 1000;
    
    [Header("Key Shop - 3 Levels")]
    [SerializeField] private int _keyRubyCost1 = 50;
    [SerializeField] private int _keyAmount1 = 5;
    
    [SerializeField] private int _keyRubyCost2 = 140;
    [SerializeField] private int _keyAmount2 = 15;
    
    [SerializeField] private int _keyRubyCost3 = 250;
    [SerializeField] private int _keyAmount3 = 30;

    public async void BuyGold(int ruby, int gold)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserRuby() >= ruby)
        {
            bool isSuccess = await FirebaseDataManager.Instance.BuyGold(ruby, gold);
            if (isSuccess)
            {
                Debug.Log("Buy Gold Success");
                UIManager.Instance.TopRightPanel.InitData();
            }
            else
            {
                Debug.Log("Buy Gold Failed");
            }
        } else UIManager.Instance.ShowNoticePopup("You don't have enough ruby!");
    }

    public async void BuyKey(int ruby, int key)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserRuby() >= ruby)
        {
            bool isSuccess = await FirebaseDataManager.Instance.BuyKey(ruby, key);
            if (isSuccess)
            {
                UIManager.Instance.TopRightPanel.InitData();
                Debug.Log("Buy Key Success");
            }
            else
            {
                Debug.Log("Buy Key Failed");
            }
        }
        else UIManager.Instance.ShowNoticePopup("You don't have enough ruby!");
    }

    // Methods for Unity OnClick events (no parameters)
    // Gold Shop - 3 Levels
    public void BuyGoldLevel1()
    {
        BuyGold(_goldRubyCost1, _goldAmount1);
    }
    
    public void BuyGoldLevel2()
    {
        BuyGold(_goldRubyCost2, _goldAmount2);
    }
    
    public void BuyGoldLevel3()
    {
        BuyGold(_goldRubyCost3, _goldAmount3);
    }

    // Key Shop - 3 Levels
    public void BuyKeyLevel1()
    {
        BuyKey(_keyRubyCost1, _keyAmount1);
    }
    
    public void BuyKeyLevel2()
    {
        BuyKey(_keyRubyCost2, _keyAmount2);
    }
    
    public void BuyKeyLevel3()
    {
        BuyKey(_keyRubyCost3, _keyAmount3);
    }
}
