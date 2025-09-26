using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemStatsDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image itemIconImage;

    [Header("Stats Text Format")]
    [SerializeField] private string damageFormat = "Sát thương: +{0}";
    [SerializeField] private string speedFormat = "Tốc độ: +{0}";
    [SerializeField] private string healthFormat = "Máu: +{0}";

    public void DisplayItemStats(InventoryItem item)
    {
        if (item == null)
        {
            ClearDisplay();
            return;
        }

        // Display basic info
        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (itemTypeText != null)
            itemTypeText.text = GetItemTypeName(item.itemType);

        if (itemIconImage != null && item.icon != null)
            itemIconImage.sprite = item.icon;

        // Display stats
        if (statsText != null)
        {
            string stats = BuildStatsString(item);
            statsText.text = stats;
        }
    }

    private string BuildStatsString(InventoryItem item)
    {
        System.Text.StringBuilder stats = new System.Text.StringBuilder();

        if (item.damageBonus > 0)
            stats.AppendLine(string.Format(damageFormat, item.damageBonus));

        if (item.speedBonus > 0)
            stats.AppendLine(string.Format(speedFormat, item.speedBonus));

        if (item.healthBonus > 0)
            stats.AppendLine(string.Format(healthFormat, item.healthBonus));

        // If no stats, show message
        if (stats.Length == 0)
            stats.AppendLine("Không có chỉ số đặc biệt");

        return stats.ToString();
    }

    private string GetItemTypeName(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                return "Mũ";
            case ItemType.Armor:
                return "Áo giáp";
            case ItemType.Gloves:
                return "Găng tay";
            case ItemType.Boots:
                return "Giày";
            case ItemType.Ring:
                return "Nhẫn";
            case ItemType.Weapon:
                return "Vũ khí";
            default:
                return "Vật phẩm";
        }
    }

    public void ClearDisplay()
    {
        if (itemNameText != null)
            itemNameText.text = "";

        if (itemTypeText != null)
            itemTypeText.text = "";

        if (statsText != null)
            statsText.text = "";

        if (itemIconImage != null)
            itemIconImage.sprite = null;
    }
}
