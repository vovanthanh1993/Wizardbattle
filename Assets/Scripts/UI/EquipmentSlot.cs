using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
public class EquipmentSlot : MonoBehaviour
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
        _itemImage.sprite = GameCommonUtils.GetItemTypeSprite(item.itemType);
        _equipmentSlotImage.color = GameCommonUtils.GetRarityColor(item.rarity);
        _item = item;
        _textEquip.SetActive(false);
    }
}
