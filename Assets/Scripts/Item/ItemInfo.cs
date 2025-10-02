using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ItemInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _itemRarityText;
    [SerializeField] private TMP_Text _itemHealthText;
    [SerializeField] private TMP_Text _itemDamageText;
    [SerializeField] private TMP_Text _itemSpeedText;
    
    [SerializeField] private TMP_Text _itemNameText;

    [SerializeField] private Image _itemSlotImage;

    [SerializeField] private Image _itemImage;
    
    [Header("Tooltip Settings")]
    [SerializeField] private RectTransform _tooltipRect;

    public void ShowItemInfo(InventoryItem item, RectTransform slotTransform = null)
    {
        if(item == null) return;
        _itemHealthText.text = item.healthBonus.ToString();
        _itemDamageText.text = item.damageBonus.ToString();
        _itemSpeedText.text = item.speedBonus.ToString();
        _itemRarityText.text = item.rarity.ToString();
        _itemNameText.text = item.itemName;
        _itemImage.sprite = GameCommonUtils.GetItemTypeSprite(item.itemType, item.materialTier);
        _itemSlotImage.color = GameCommonUtils.GetRarityColor(item.rarity);

        if(slotTransform != null) {
            Vector2 slotSize = slotTransform.sizeDelta;
            Vector3 topRightPosition = slotTransform.position + new Vector3(slotSize.x / 2.2f, slotSize.y / 2.2f, 0);
            _tooltipRect.position = topRightPosition;
            
            // Keep tooltip within screen bounds
            KeepTooltipInBounds();
        }
        gameObject.SetActive(true);
    }

    public void HideItemInfo()
    {
        gameObject.SetActive(false);
    }
    
    private void KeepTooltipInBounds()
    {
        if (_tooltipRect == null) return;
        
        // Get screen bounds
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Vector2 tooltipSize = _tooltipRect.sizeDelta;
        
        // Get current position
        Vector3 currentPos = _tooltipRect.position;
        
        // Check right edge
        if (currentPos.x + tooltipSize.x > screenSize.x)
        {
            currentPos.x = screenSize.x - tooltipSize.x - 10; // 10px margin
        }
        
        // Check left edge
        if (currentPos.x < 0)
        {
            currentPos.x = 10; // 10px margin
        }
        
        // Check top edge
        if (currentPos.y + tooltipSize.y > screenSize.y)
        {
            currentPos.y = screenSize.y - tooltipSize.y - 10; // 10px margin
        }
        
        // Check bottom edge
        if (currentPos.y < 0)
        {
            currentPos.y = 10; // 10px margin
        }
        
        _tooltipRect.position = currentPos;
    }
}
