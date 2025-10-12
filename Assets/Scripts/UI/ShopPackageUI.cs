using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopPackageUI : MonoBehaviour
{

    [SerializeField] private Button _shopButton;
    [SerializeField] private TMP_Text _shopDescription;
    [SerializeField] private TMP_Text _shopPrice;
    [SerializeField] private Image _shopImage;

    private ShopData _shopData;

    public void SetShopPackage(ShopData shopData)
    {
        _shopDescription.text = "+" + shopData.buyAmount + " " + shopData.buyType.ToString();
        if (shopData.buyType == ShopType.Ruby)
        {
            _shopPrice.text = shopData.paidAmount.ToString() + "$";
        }
        else
        {
            _shopPrice.text = shopData.paidAmount.ToString();
        }

        // Load image using FirebaseImageHelper
        if (FirebaseImageHelper.Instance != null && _shopImage != null && !string.IsNullOrEmpty(shopData.imageURL))
        {
            FirebaseImageHelper.Instance.LoadImageToComponent(_shopImage, shopData.imageURL);
            _shopImage.rectTransform.sizeDelta = new Vector2(shopData.width, shopData.height);
        }
        else
        {
            Debug.LogError("ShopPackageUI: FirebaseImageHelper.Instance, _shopImage, or imageURL is null!");
        }
        _shopButton.onClick.AddListener(HandleShopButtonClicked);
        _shopData = shopData;
    }

    private void HandleShopButtonClicked()
    {
        switch (_shopData.buyType)
        {
            case ShopType.Gold:
                BuyGold(_shopData.paidAmount, _shopData.buyAmount);
                break;
            case ShopType.Food:
                BuyFood(_shopData.paidAmount, _shopData.buyAmount);
                break;
            case ShopType.Ruby:
                BuyRuby(_shopData.paidAmount, _shopData.buyAmount);
                break;
        }
    }

    public async void BuyGold(float ruby, int gold)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserRuby() >= ruby)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            bool isSuccess = await FirebaseDataManager.Instance.BuyGold((int)ruby, gold);
            if (isSuccess)
            {
                Debug.Log("Buy Gold Success");
                UIManager.Instance.ShowNoticePopup($"Buy {gold} Gold Success!");
                UIManager.Instance.TopRightPanel.InitData();
                GameEvents.TriggerUpdateShopNumbers();
                UIManager.Instance.ShowLoadingPanel(false);
                AudioManager.Instance.PlayBuySuccessSound();
            }
            else
            {
                Debug.Log("Buy Gold Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup("Buy Gold Failed! Please try again.");
            }
        } else {
            UIManager.Instance.ShowNoticePopup("You don't have enough ruby!");
            AudioManager.Instance.PlayNotEnoughSound();
        }
    }

    public async void BuyFood(float ruby, int food)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserRuby() >= ruby)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            bool isSuccess = await FirebaseDataManager.Instance.BuyFood((int)ruby, food);
            if (isSuccess)
            {
                UIManager.Instance.TopRightPanel.InitData();
                GameEvents.TriggerUpdateShopNumbers();
                Debug.Log("Buy Food Success");
                UIManager.Instance.ShowNoticePopup($"Buy {food} Food Success!");
                UIManager.Instance.ShowLoadingPanel(false);
                AudioManager.Instance.PlayBuySuccessSound();
            }
            else
            {
                Debug.Log("Buy Food Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup("Buy Food Failed! Please try again.");
            }
        }
        else {
            UIManager.Instance.ShowNoticePopup("You don't have enough ruby!");
            AudioManager.Instance.PlayNotEnoughSound();
        }
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
                GameEvents.TriggerUpdateShopNumbers();
                Debug.Log("Buy Ruby Success");
                UIManager.Instance.ShowNoticePopup($"Buy {ruby} Ruby Success!");
                UIManager.Instance.ShowLoadingPanel(false);
                AudioManager.Instance.PlayBuySuccessSound();
            }
            else
            {
                Debug.Log("Buy Ruby Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup("Buy Ruby Failed! Please try again.");
            }
        }
        else {
            UIManager.Instance.ShowNoticePopup("You don't have enough cash!");
            AudioManager.Instance.PlayNotEnoughSound();
        }
    }
}
