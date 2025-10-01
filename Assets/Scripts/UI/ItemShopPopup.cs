using UnityEngine;
using TMPro;

public class ItemShopPopup : MonoBehaviour
{
    [Header("Number")]
    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private int _goldCost = 200;
    [SerializeField] private int _randomCost = 150;

    [SerializeField] GameObject _shopItemList;
    
    private int _currentItemIndex = 0;

    public void UpdateNumber()
    { 
        if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserGold().ToString();
    }
    
    private void OnEnable()
    {
        UpdateNumber();
        AudioManager.Instance.PlayButtonPopupSound();
        InitializeItemDisplay();
    }
    
    private void InitializeItemDisplay()
    {
        if (_shopItemList == null) return;
        
        int childCount = _shopItemList.transform.childCount;
        if (childCount == 0) return;
        
        // Hide all items first
        for (int i = 0; i < childCount; i++)
        {
            _shopItemList.transform.GetChild(i).gameObject.SetActive(false);
        }
        
        // Show first item
        _currentItemIndex = 0;
        _shopItemList.transform.GetChild(_currentItemIndex).gameObject.SetActive(true);
        
        Debug.Log($"Initialized item display. Showing item {_currentItemIndex + 1} of {childCount}");
    }

    public async void BuyRandomItem(int gold, ItemType itemType)
    {
        if (FirebaseDataManager.Instance.GetCurrentUserGold() >= gold)
        {
            UIManager.Instance.ShowLoadingPanel(true);
            var creator = Resources.Load<ItemCreator>("ItemCreator");
            if (creator == null)
            {
                creator = ScriptableObject.CreateInstance<ItemCreator>();
            }

            // Create random item with random stats for specific type
            InventoryItem randomItem = creator.CreateRandomItem(itemType);
            bool isSuccess = await FirebaseDataManager.Instance.BuyRandomItem(gold, randomItem);
            if (isSuccess)
            {
                UIManager.Instance.ItemInfo.ShowItemInfo(randomItem);
                UIManager.Instance.TopRightPanel.InitData();
                UpdateNumber();
                UIManager.Instance.ShowLoadingPanel(false);
                AudioManager.Instance.PlayBuySuccessSound();
            }
            else
            {
                Debug.Log($"Buy Random {itemType} Failed");
                UIManager.Instance.ShowLoadingPanel(false);
                UIManager.Instance.ShowNoticePopup($"Buy Random {itemType} Failed! Please try again.");
            }
        } else {
            UIManager.Instance.ShowNoticePopup("You don't have enough gold!");
            AudioManager.Instance.PlayNotEnoughSound();
        }
    }

    // Convenience methods for different item types
    public void BuyRandomArmor()
    {
        BuyRandomItem(_goldCost, ItemType.Armor);
    }

    public void BuyRandomWeapon()
    {
        BuyRandomItem(_goldCost, ItemType.Weapon);
    }

    public void BuyRandomHelmet()
    {
        BuyRandomItem(_goldCost, ItemType.Helmet);
    }

    public void BuyRandomGloves()
    {
        BuyRandomItem(_goldCost, ItemType.Gloves);
    }

    public void BuyRandomBoots()
    {
        BuyRandomItem(_goldCost, ItemType.Boots);
    }

    public void BuyRandomRing()
    {
        BuyRandomItem(_goldCost, ItemType.Ring);
    }

    public void BuyRandomAmulet()
    {
        BuyRandomItem(_goldCost, ItemType.Amulet);
    }

    public void BuyRandomBook()
    {
        BuyRandomItem(_goldCost, ItemType.Book);
    }

    public void BuyRandom()
    {
        ItemType itemType = (ItemType)Random.Range(0, 8);
        BuyRandomItem(_randomCost, itemType);
    }

    public void NextItem()
    {
        if (_shopItemList == null) return;
        
        int childCount = _shopItemList.transform.childCount;
        if (childCount == 0) return;
        
        // Hide current item
        if (_currentItemIndex < childCount)
        {
            _shopItemList.transform.GetChild(_currentItemIndex).gameObject.SetActive(false);
        }
        
        // Move to next item
        _currentItemIndex = (_currentItemIndex + 1) % childCount;
        
        // Show next item
        _shopItemList.transform.GetChild(_currentItemIndex).gameObject.SetActive(true);
        
        Debug.Log($"Showing item {_currentItemIndex + 1} of {childCount}");
    }

    public void PreviousItem()
    {
        if (_shopItemList == null) return;
        
        int childCount = _shopItemList.transform.childCount;
        if (childCount == 0) return;
        
        // Hide current item
        if (_currentItemIndex < childCount)
        {
            _shopItemList.transform.GetChild(_currentItemIndex).gameObject.SetActive(false);
        }
        
        // Move to previous item
        _currentItemIndex = (_currentItemIndex - 1 + childCount) % childCount;
        
        // Show previous item
        _shopItemList.transform.GetChild(_currentItemIndex).gameObject.SetActive(true);
        
        Debug.Log($"Showing item {_currentItemIndex + 1} of {childCount}");
    }
}
