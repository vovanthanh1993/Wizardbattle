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
        var masterMaterialTier = GetRandomMaterialTier();
        var itemName = masterMaterialTier + " " + itemType;
        var item = ScriptableObject.CreateInstance<InventoryItem>();
        item.itemType = itemType;
        item.itemId = itemName + UnityEngine.Random.Range(1, 1000000);
        item.itemName = itemName;
        item.isStackable = false;
        item.maxStack = 1;
        item.rarity = GetRandomRarity();
        item.materialTier = masterMaterialTier;
        // Generate random stats based on item type
        GenerateRandomStats(item, itemType);

        return item;
    }

    private void GenerateRandomStats(InventoryItem item, ItemType itemType)
    {
        float rarityMultiplier = GetRarityMultiplier(item.rarity);
        float materialTierMultiplier = GetMaterialTierMultiplier(item.materialTier);
        float combinedMultiplier = rarityMultiplier * materialTierMultiplier;
        
        switch (itemType)
        {
            case ItemType.Weapon:
                // Weapon - High damage, low health and speed
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(12f, 28f) * combinedMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3, 8) * combinedMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(1f, 4f) * combinedMultiplier);
                break;

            case ItemType.Helmet:
                // Helmet - Medium health, low damage and speed
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(15, 35) * combinedMultiplier);
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 8f) * combinedMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(2f, 5f) * combinedMultiplier);
                break;

            case ItemType.Armor:
                // Armor - Very high health, medium damage and speed
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(30, 70) * combinedMultiplier);
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(6f, 18f) * combinedMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 9f) * combinedMultiplier);
                break;

            case ItemType.Gloves:
                // Gloves - Medium damage, low speed and health
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(6f, 15f) * combinedMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 6f) * combinedMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(5, 12) * combinedMultiplier);
                break;

            case ItemType.Boots:
                // Boots - High speed, low damage and health
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(6f, 15f) * combinedMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(5, 12) * combinedMultiplier);
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 8f) * combinedMultiplier);
                break;

            case ItemType.Ring:
                // Ring - All stats low, balanced
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(6, 15) * combinedMultiplier);
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 9f) * combinedMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(2f, 5f) * combinedMultiplier);
                break;

            case ItemType.Amulet:
                // Amulet - Good damage and health, medium speed
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(8f, 22f) * combinedMultiplier);
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(12, 28) * combinedMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(3f, 7f) * combinedMultiplier);
                break;

            case ItemType.Book:
                // Book - Good health, medium damage, low speed
                item.healthBonus = Mathf.RoundToInt(UnityEngine.Random.Range(15, 40) * combinedMultiplier);
                item.damageBonus = Mathf.RoundToInt(UnityEngine.Random.Range(5f, 13f) * combinedMultiplier);
                item.speedBonus = Mathf.RoundToInt(UnityEngine.Random.Range(2f, 5f) * combinedMultiplier);
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
            ItemType.Amulet => new string[] { "Protection Amulet", "Power Amulet", "Speed Amulet", "Health Amulet", "Magic Amulet" },
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

    /// <summary>
    /// Get stat multiplier based on material tier
    /// </summary>
    private float GetMaterialTierMultiplier(MaterialTier materialTier)
    {
        switch (materialTier)
        {
            case MaterialTier.Bronze:
                return 0.8f;  // 80% of base stats
            case MaterialTier.Iron:
                return 1.0f;  // 100% of base stats
            case MaterialTier.Silver:
                return 1.3f;  // 130% of base stats
            case MaterialTier.Gold:
                return 1.6f;  // 160% of base stats
            case MaterialTier.Mythril:
                return 2.0f;  // 200% of base stats - Highest tier
            default:
                return 1.0f;
        }
    }

    /// <summary>
    /// Get random material tier with weighted probability
    /// </summary>
    private MaterialTier GetRandomMaterialTier()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        
        // Weighted probability: Bronze 35%, Iron 30%, Silver 20%, Gold 10%, Mythril 5%
        if (randomValue < 0.35f)
            return MaterialTier.Bronze;
        else if (randomValue < 0.65f)
            return MaterialTier.Iron;
        else if (randomValue < 0.85f)
            return MaterialTier.Silver;
        else if (randomValue < 0.95f)
            return MaterialTier.Gold;
        else
            return MaterialTier.Mythril;
    }
}
