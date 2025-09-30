using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class EquipmentSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _equipmentSlotImage;

    [SerializeField] private GameObject _textEquip;
    private InventoryItem _item;

    public void SetData(InventoryItem item)
    {
        if(item == null) {
            _itemImage.gameObject.SetActive(false);
            _equipmentSlotImage.color = Color.white;
            _item = null;
            _textEquip.SetActive(true);
            return;
        }
        _itemImage.gameObject.SetActive(true);
        _itemImage.sprite = GameCommonUtils.GetItemTypeSprite(item.itemType, item.materialTier);
        _equipmentSlotImage.color = GameCommonUtils.GetRarityColor(item.rarity);
        _item = item;
        _textEquip.SetActive(false);
    }

    // Hover events
    public void OnPointerEnter(PointerEventData eventData)
    {
        //_slotBorder.SetActive(true);
        UIManager.Instance.ItemToolTip.ShowItemInfo(_item, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //_slotBorder.SetActive(false);
        UIManager.Instance.ItemToolTip.HideItemInfo();
    }
}
