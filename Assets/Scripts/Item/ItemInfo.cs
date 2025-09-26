using UnityEngine;
using TMPro;
public class ItemInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _itemRarityText;
    [SerializeField] private TMP_Text _itemHealthText;
    [SerializeField] private TMP_Text _itemDamageText;
    [SerializeField] private TMP_Text _itemSpeedText;
    
    [SerializeField] private TMP_Text _itemNameText;

    public void ShowItemInfo(InventoryItem item)
    {
        gameObject.SetActive(true);
        _itemHealthText.text = "+" + item.healthBonus.ToString();
        _itemDamageText.text = "+" + item.damageBonus.ToString();
        _itemSpeedText.text = "+" + item.speedBonus.ToString();
        _itemRarityText.text = item.rarity.ToString();
        _itemNameText.text = item.itemName;
    }

    public void HideItemInfo()
    {
        gameObject.SetActive(false);
    }
}
