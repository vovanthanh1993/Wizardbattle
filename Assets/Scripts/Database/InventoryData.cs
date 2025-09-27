using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;



[Serializable]
public class InventoryData
{
    [Header("Inventory Items")]
    public List<InventoryItemData> items = new List<InventoryItemData>();
    
    [Header("Equipment")]
    public string equippedHelmetId;
    public string equippedArmorId;
    public string equippedGlovesId;
    public string equippedBootsId;
    public string equippedRingId;
    public string equippedWeaponId;
    public string equippedAmuletId;
    public string equippedBookId;

    public InventoryData()
    {
        items = new List<InventoryItemData>();
        equippedHelmetId = "";
        equippedArmorId = "";
        equippedGlovesId = "";
        equippedBootsId = "";
        equippedRingId = "";
        equippedWeaponId = "";
        equippedAmuletId = "";
        equippedBookId = "";
    }

    public void AddItem(string itemId, int quantity = 1)
    {
        var existingItem = items.Find(x => x.itemId == itemId);
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            items.Add(new InventoryItemData(itemId, quantity));
        }
    }

    public void AddRandomItem(InventoryItem randomItem, int quantity = 1)
    {
        var existingItem = items.Find(x => x.itemId == randomItem.itemId);
        if (existingItem != null)
        {
            existingItem.quantity += quantity;
        }
        else
        {
            var itemData = new InventoryItemData(randomItem, quantity);
            items.Add(itemData);
        }
    }

    public bool RemoveItem(string itemId, int quantity = 1)
    {
        var existingItem = items.Find(x => x.itemId == itemId);
        if (existingItem != null)
        {
            existingItem.quantity -= quantity;
            if (existingItem.quantity <= 0)
            {
                items.Remove(existingItem);
            }
            return true;
        }
        return false;
    }

    public int GetItemQuantity(string itemId)
    {
        var existingItem = items.Find(x => x.itemId == itemId);
        return existingItem?.quantity ?? 0;
    }

    public void EquipItem(string itemId, ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                equippedHelmetId = itemId;
                break;
            case ItemType.Armor:
                equippedArmorId = itemId;
                break;
            case ItemType.Gloves:
                equippedGlovesId = itemId;
                break;
            case ItemType.Boots:
                equippedBootsId = itemId;
                break;
            case ItemType.Ring:
                equippedRingId = itemId;
                break;
            case ItemType.Weapon:
                equippedWeaponId = itemId;
                break;
            case ItemType.Amulet:
                equippedAmuletId = itemId;
                break;
            case ItemType.Book:
                equippedBookId = itemId;
                break;
        }
    }

    public void UnequipItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                equippedHelmetId = "";
                break;
            case ItemType.Armor:
                equippedArmorId = "";
                break;
            case ItemType.Gloves:
                equippedGlovesId = "";
                break;
            case ItemType.Boots:
                equippedBootsId = "";
                break;
            case ItemType.Ring:
                equippedRingId = "";
                break;
            case ItemType.Weapon:
                equippedWeaponId = "";
                break;
            case ItemType.Amulet:
                equippedAmuletId = "";
                break;
            case ItemType.Book:
                equippedBookId = "";
                break;
        }
    }

    public string GetEquippedItemId(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                return equippedHelmetId;
            case ItemType.Armor:
                return equippedArmorId;
            case ItemType.Gloves:
                return equippedGlovesId;
            case ItemType.Boots:
                return equippedBootsId;
            case ItemType.Ring:
                return equippedRingId;
            case ItemType.Weapon:
                return equippedWeaponId;
            case ItemType.Amulet:
                return equippedAmuletId;
            case ItemType.Book:
                return equippedBookId;
            default:
                return "";
        }
    }

    public List<InventoryItem> GetItemsByType(ItemType itemType)
    {
        List<InventoryItem> result = new List<InventoryItem>();
        
        foreach (var itemData in items)
        {
            if (itemData.itemType == itemType)
            {
                result.Add(itemData.ToInventoryItem());
                Debug.Log("Added item: " + itemData.itemName);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Get all equipped item IDs as a list
    /// </summary>
    public List<string> GetAllEquippedItemIds()
    {
        return new List<string>
        {
            equippedHelmetId,
            equippedArmorId,
            equippedGlovesId,
            equippedBootsId,
            equippedRingId,
            equippedWeaponId,
            equippedAmuletId,
            equippedBookId
        }.Where(id => !string.IsNullOrEmpty(id)).ToList();
    }
    
    /// <summary>
    /// Check if an item is equipped using LINQ (alternative approach)
    /// </summary>
    public bool IsItemEquipped(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;
        return GetAllEquippedItemIds().Contains(itemId);
    }
}

[Serializable]
public class InventoryItemData
{
    public string itemId;
    public int quantity;

    public string itemName;
    public ItemType itemType;
    public Rarity rarity;
    public MaterialTier materialTier;
    public int damageBonus;
    public int speedBonus;
    public int healthBonus;
    public bool isStackable;
    public int maxStack;

    public InventoryItemData(string itemId, int quantity = 1)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        this.isStackable = false;
        this.maxStack = 1;
    }

    public InventoryItemData(InventoryItem item, int quantity = 1)
    {
        this.itemId = item.itemId;
        this.quantity = quantity;
        this.itemName = item.itemName;
        this.itemType = item.itemType;
        this.rarity = item.rarity;
        this.materialTier = item.materialTier;
        this.damageBonus = item.damageBonus;
        this.speedBonus = item.speedBonus;
        this.healthBonus = item.healthBonus;
        this.isStackable = item.isStackable;
        this.maxStack = item.maxStack;
    }

    public InventoryItem ToInventoryItem()
    {
        InventoryItem item = ScriptableObject.CreateInstance<InventoryItem>();
        item.itemId = itemId;
        item.itemName = itemName;
        item.itemType = itemType;
        item.rarity = rarity;
        item.materialTier = materialTier;
        item.damageBonus = damageBonus;
        item.speedBonus = speedBonus;
        item.healthBonus = healthBonus;
        item.isStackable = isStackable;
        item.maxStack = maxStack;
        return item;
    }
}
