using UnityEngine;
using TMPro;

public enum ShopItemType
{
    Ruby,
    Gold,
    Food
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
    
    [Header("Food Shop - 3 Levels")]
    [SerializeField] private int _foodRubyCost1 = 50;
    [SerializeField] private int _foodAmount1 = 5;
    
    [SerializeField] private int _foodRubyCost2 = 140;
    [SerializeField] private int _foodAmount2 = 15;
    
    [SerializeField] private int _foodRubyCost3 = 250;
    [SerializeField] private int _foodAmount3 = 30;

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

    public async void BuyFood(int ruby, int food)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserRuby() >= ruby)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            bool isSuccess = await FirebaseDataManager.Instance.BuyFood(ruby, food);
            if (isSuccess)
            {
                UIManager.Instance.TopRightPanel.InitData();
                UpdateNumber();
                Debug.Log("Buy Food Success");
                UIManager.Instance.ShowNoticePopup($"Buy {food} Food Success!");
                UIManager.Instance.ShowLoadingPanel(false);
            }
            else
            {
                Debug.Log("Buy Food Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup("Buy Food Failed! Please try again.");
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

    // Food Shop - 3 Levels
    public void BuyFoodLevel1()
    {
        BuyFood(_foodRubyCost1, _foodAmount1);
    }
    
    public void BuyFoodLevel2()
    {
        BuyFood(_foodRubyCost2, _foodAmount2);
    }
    
    public void BuyFoodLevel3()
    {
        BuyFood(_foodRubyCost3, _foodAmount3);
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
            case ShopItemType.Food:
                if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserFood().ToString();
                break;
        }
    }
    private void OnEnable()
    {
        UpdateNumber();
    }
}
