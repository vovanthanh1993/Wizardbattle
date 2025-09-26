using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private GameObject _slotBorder;
    [SerializeField] private Image _itemSlotImage;
    [SerializeField] private GameObject _inUseImage;
    private InventoryItem _item;
    
    // Static event để thông báo khi có slot được chọn
    public static event Action<ItemSlot> OnSlotSelected;


    private void OnEnable()
    {
        // Subscribe to selection event
        OnSlotSelected += OnOtherSlotSelected;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from selection event
        OnSlotSelected -= OnOtherSlotSelected;
    }
    
    public void SetData(InventoryItem item)
    {   
        _itemImage.sprite = GameCommonUtils.GetItemTypeSprite(item.itemType);
        _slotBorder.SetActive(false);
        _itemSlotImage.color = GameCommonUtils.GetRarityColor(item.rarity);
        _item = item;
        PlayerData playerData = FirebaseDataManager.Instance.GetCurrentPlayerData();
        _inUseImage.SetActive(playerData.inventoryData.IsItemEquipped(item.itemId));
        
    }

    // Hover events
    public void OnPointerEnter(PointerEventData eventData)
    {
        _slotBorder.SetActive(true);
        UIManager.Instance.ItemToolTip.ShowItemInfo(_item);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _slotBorder.SetActive(false);
        UIManager.Instance.ItemToolTip.HideItemInfo();
    }

    public void OnClickItemSlot()
    {
        // Activate this slot's selection
        _inUseImage.SetActive(true);
        
        UIManager.Instance.InventoryPanel.EquipItem(_item);
        EquipmentManager.Instance.EquipItem(_item);
        
        // Notify all other slots that this one was selected
        OnSlotSelected?.Invoke(this);
    }
    
    // Called when another slot is selected
    private void OnOtherSlotSelected(ItemSlot selectedSlot)
    {
        // If this is not the selected slot, deactivate selection
        if (selectedSlot != this)
        {
            _inUseImage.SetActive(false);
        }
    }
}
