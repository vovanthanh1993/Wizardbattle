using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ItemCreator", menuName = "Inventory/Item Creator")]
public class ItemCreator : ScriptableObject
{
    [Header("Item Templates")]
    [SerializeField] private InventoryItem[] itemTemplates;

    /// <summary>
    /// Create a random item with random stats
    /// </summary>
    public InventoryItem CreateRandomItem(ItemType itemType)
    {
        var itemName = GetRandomItemName(itemType);
        var item = ScriptableObject.CreateInstance<InventoryItem>();
        item.itemType = itemType;
        item.itemId = itemName + UnityEngine.Random.Range(1, 1000000);
        item.itemName = itemName;
        item.isStackable = false;
        item.maxStack = 1;
        item.rarity = GetRandomRarity();

        // Generate random stats based on item type
        GenerateRandomStats(item, itemType);

        return item;
    }

    private void GenerateRandomStats(InventoryItem item, ItemType itemType)
    {
        float rarityMultiplier = GetRarityMultiplier(item.rarity);
        
        switch (itemType)
        {
            case ItemType.Weapon:
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(10f, 50f) * rarityMultiplier);
                break;

            case ItemType.Helmet:
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(20, 80) * rarityMultiplier);
                break;

            case ItemType.Armor:
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(50, 150) * rarityMultiplier);
                break;

            case ItemType.Gloves:
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(5f, 25f) * rarityMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(2f, 8f) * rarityMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(10, 40) * rarityMultiplier);
                break;

            case ItemType.Boots:
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(5f, 15f) * rarityMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(10, 40) * rarityMultiplier);
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(5f, 25f) * rarityMultiplier);
                break;

            case ItemType.Ring:
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(10, 40) * rarityMultiplier);
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 15f) * rarityMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(1f, 5f) * rarityMultiplier);
                break;

            case ItemType.Rune:
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(8f, 30f) * rarityMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 12f) * rarityMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(15, 60) * rarityMultiplier);
                break;

            case ItemType.Book:
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(5f, 20f) * rarityMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(2f, 8f) * rarityMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(20, 80) * rarityMultiplier);
                break;
        }
    }

    private string GetRandomItemName(ItemType itemType)
    {
        string[] names = itemType switch
        {
            ItemType.Weapon => new string[] { "Iron Sword", "Magic Staff", "Divine Bow", "Battle Axe", "Dagger" },
            ItemType.Helmet => new string[] { "Iron Helmet", "Magic Helm", "Divine Crown", "Battle Cap", "Shadow Hood" },
            ItemType.Armor => new string[] { "Iron Armor", "Magic Robe", "Divine Plate", "Battle Mail", "Shadow Cloak" },
            ItemType.Gloves => new string[] { "Iron Gauntlets", "Magic Gloves", "Divine Bracers", "Battle Grips", "Shadow Mitts" },
            ItemType.Boots => new string[] { "Iron Boots", "Magic Slippers", "Divine Greaves", "Battle Stompers", "Shadow Treads" },
            ItemType.Ring => new string[] { "Iron Ring", "Magic Band", "Divine Circle", "Battle Loop", "Shadow Band" },
            ItemType.Rune => new string[] { "Fire Rune", "Ice Rune", "Lightning Rune", "Earth Rune", "Wind Rune" },
            ItemType.Book => new string[] { "Spellbook", "Ancient Tome", "Magic Scroll", "Mystic Map", "Library Codex" },
            _ => new string[] { "Mysterious Item" }
        };

        return names[UnityEngine.Random.Range(0, names.Length)];
    }

    /// <summary>
    /// Get random rarity with weighted probability
    /// </summary>
    private Rarity GetRandomRarity()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        
        // Weighted probability: Common 60%, Rare 25%, Epic 12%, Legendary 3%
        if (randomValue < 0.6f)
            return Rarity.Common;
        else if (randomValue < 0.85f)
            return Rarity.Rare;
        else if (randomValue < 0.97f)
            return Rarity.Epic;
        else
            return Rarity.Legendary;
    }

    /// <summary>
    /// Get stat multiplier based on rarity
    /// </summary>
    private float GetRarityMultiplier(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return 1.0f;
            case Rarity.Rare:
                return 1.5f;
            case Rarity.Epic:
                return 2.0f;
            case Rarity.Legendary:
                return 3.0f;
            default:
                return 1.0f;
        }
    }
}
