using UnityEngine;
using TMPro;

public enum ShopItemType
{
    Ruby,
    Gold,
    Key
}
public class ShopManager : MonoBehaviour
{

    [Header("Number")]
    [SerializeField] private ShopItemType _shopItemType;
    [SerializeField] private TMP_Text _numberText;
    
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

    [Header("Ruby Shop - 3 Levels")]
    [SerializeField] private float _cashCost1 = 0.99f;
    [SerializeField] private int _rubyAmount1 = 50;
    
    [SerializeField] private float _cashCost2 = 1.99f;
    [SerializeField] private int _rubyAmount2 = 200;
    
    [SerializeField] private float _cashCost3 = 2.5f;
    [SerializeField] private int _rubyAmount3 = 500;

    public async void BuyGold(int ruby, int gold)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserRuby() >= ruby)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            bool isSuccess = await FirebaseDataManager.Instance.BuyGold(ruby, gold);
            if (isSuccess)
            {
                Debug.Log("Buy Gold Success");
                UIManager.Instance.ShowNoticePopup($"Buy {gold} Gold Success!");
                UIManager.Instance.TopRightPanel.InitData();
                UpdateNumber();
                UIManager.Instance.ShowLoadingPanel(false);
            }
            else
            {
                Debug.Log("Buy Gold Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup("Buy Gold Failed! Please try again.");
            }
        } else UIManager.Instance.ShowNoticePopup("You don't have enough ruby!");
    }

    public async void BuyKey(int ruby, int key)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserRuby() >= ruby)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            bool isSuccess = await FirebaseDataManager.Instance.BuyKey(ruby, key);
            if (isSuccess)
            {
                UIManager.Instance.TopRightPanel.InitData();
                UpdateNumber();
                Debug.Log("Buy Key Success");
                UIManager.Instance.ShowNoticePopup($"Buy {key} Key Success!");
                UIManager.Instance.ShowLoadingPanel(false);
            }
            else
            {
                Debug.Log("Buy Key Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup("Buy Key Failed! Please try again.");
            }
        }
        else UIManager.Instance.ShowNoticePopup("You don't have enough ruby!");
    }


    public async void BuyRuby(float cash, int ruby)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserCash() >= cash)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            bool isSuccess = await FirebaseDataManager.Instance.BuyRuby(cash, ruby);
            if (isSuccess)
            {
                UIManager.Instance.TopRightPanel.InitData();
                UpdateNumber();
                Debug.Log("Buy Ruby Success");
                UIManager.Instance.ShowNoticePopup($"Buy {ruby} Ruby Success!");
                UIManager.Instance.ShowLoadingPanel(false);
            }
            else
            {
                Debug.Log("Buy Ruby Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup("Buy Ruby Failed! Please try again.");
            }
        }
        else UIManager.Instance.ShowNoticePopup("You don't have enough cash!");
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

    public void BuyRubyLevel1()
    {
        BuyRuby(_cashCost1, _rubyAmount1);
    }

    public void BuyRubyLevel2()
    {
        BuyRuby(_cashCost2, _rubyAmount2);
    }

    public void BuyRubyLevel3()
    {
        BuyRuby(_cashCost3, _rubyAmount3);
    }

    public void UpdateNumber()
    {
        switch (_shopItemType)
        {
            case ShopItemType.Ruby:
                if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserRuby().ToString();
                break;
            case ShopItemType.Gold:
                if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserGold().ToString();
                break;
            case ShopItemType.Key:
                if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserKey().ToString();
                break;
        }
    }
    
    private void Start()
    {
        UpdateNumber();
    }
}
