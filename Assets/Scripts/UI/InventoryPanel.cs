using UnityEngine;
using TMPro;

public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private ItemPanel _itemPanel;

    [SerializeField] private EquipmentSlot _helmetSlot;
    [SerializeField] private EquipmentSlot _armorSlot;
    [SerializeField] private EquipmentSlot _glovesSlot;
    [SerializeField] private EquipmentSlot _bootsSlot;
    [SerializeField] private EquipmentSlot _weaponSlot;
    [SerializeField] private EquipmentSlot _bookSlot;
    [SerializeField] private EquipmentSlot _ringSlot;
    [SerializeField] private EquipmentSlot _amuletSlot;

    [SerializeField] private GameObject _equipmentPanel;

    [SerializeField] private TMP_Text _healthText;
    [SerializeField] private TMP_Text _damageText;
    [SerializeField] private TMP_Text _speedText;
    
    private void OnEnable() {
        InitData();
        // Subscribe to equipment change events
        EquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
    }
    
    private void OnDisable() {
        // Unsubscribe from equipment change events
        EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
    }
    
    private void OnEquipmentChanged() {
        InitData();
    }
    public void ShowAllArmor() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Armor);
    }

    public void ShowAllGloves() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Gloves);
    }

    public void ShowAllBoots() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Boots);
    }

    public void ShowAllRing() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Ring);
    }
    
    public void ShowAllWeapon() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Weapon);
    }

    public void ShowAllAmulet() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Amulet);
    }

    public void ShowAllBook() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Book);
    }
    
    public void ShowAllHelmet() {
        UIManager.Instance.ItemToolTip.HideItemInfo();
        _equipmentPanel.SetActive(false);
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Helmet);
    }

    public void EquipItem(InventoryItem item) {
        switch (item.itemType) {
            case ItemType.Helmet:
                _helmetSlot.SetData(item);
                break;
            case ItemType.Armor:
                _armorSlot.SetData(item);
                break;
            case ItemType.Gloves:
                _glovesSlot.SetData(item);
                break;
            case ItemType.Boots:
                _bootsSlot.SetData(item);
                break;
            case ItemType.Ring:
                _ringSlot.SetData(item);
                break;
            case ItemType.Weapon:
                _weaponSlot.SetData(item);
                break;
            case ItemType.Amulet:
                _amuletSlot.SetData(item);
                break;
            case ItemType.Book:
                _bookSlot.SetData(item);
                break;
        }
    }

    public void InitData() {
        _helmetSlot.SetData(EquipmentManager.Instance.equippedHelmet);
        _armorSlot.SetData(EquipmentManager.Instance.equippedArmor);
        _glovesSlot.SetData(EquipmentManager.Instance.equippedGloves);
        _bootsSlot.SetData(EquipmentManager.Instance.equippedBoots);
        _ringSlot.SetData(EquipmentManager.Instance.equippedRing);
        _weaponSlot.SetData(EquipmentManager.Instance.equippedWeapon);
        _amuletSlot.SetData(EquipmentManager.Instance.equippedAmulet);
        _bookSlot.SetData(EquipmentManager.Instance.equippedBook);
        _healthText.text = FirebaseDataManager.Instance.GetCurrentUserHealth().ToString();
        _damageText.text = FirebaseDataManager.Instance.GetCurrentUserDamage().ToString();
        _speedText.text = FirebaseDataManager.Instance.GetCurrentUserSpeed().ToString();
    }
}
