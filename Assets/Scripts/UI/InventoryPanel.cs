using UnityEngine;

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
    [SerializeField] private EquipmentSlot _runeSlot;
    
    private void OnEnable() {
        InitData();
    }
    public void ShowAllArmor() {
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Armor);
    }

    public void ShowAllGloves() {
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Gloves);
    }

    public void ShowAllBoots() {
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Boots);
    }

    public void ShowAllRing() {
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Ring);
    }
    
    public void ShowAllWeapon() {
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Weapon);
    }

    public void ShowAllRune() {
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Rune);
    }

    public void ShowAllBook() {
        _itemPanel.gameObject.SetActive(true);
        _itemPanel.ShowItemPanel(ItemType.Book);
    }
    
    public void ShowAllHelmet() {
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
            case ItemType.Rune:
                _runeSlot.SetData(item);
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
        _runeSlot.SetData(EquipmentManager.Instance.equippedRune);
        _bookSlot.SetData(EquipmentManager.Instance.equippedBook);
        
    }
}
