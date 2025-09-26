using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class EquipmentManager : MonoBehaviour
{
    [Header("Equipment Slots")]
    public InventoryItem equippedHelmet;
    public InventoryItem equippedArmor;
    public InventoryItem equippedGloves;
    public InventoryItem equippedBoots;
    public InventoryItem equippedRing;
    public InventoryItem equippedWeapon;
    public InventoryItem equippedRune;
    public InventoryItem equippedBook;

    public static EquipmentManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // Calculated stats from all equipped items
    public int TotalDamageBonus { get; private set; }
    public int TotalSpeedBonus { get; private set; }
    public int TotalHealthBonus { get; private set; }

    private void Start()
    {   
        InitData();
    }

    /// <summary>
    /// Equip an item to the appropriate slot
    /// </summary>
    public bool EquipItem(InventoryItem item)
    {
        if (item == null) return false;

        switch (item.itemType)
        {
            case ItemType.Helmet:
                equippedHelmet = item;
                break;
            case ItemType.Armor:
                equippedArmor = item;
                break;
            case ItemType.Gloves:
                equippedGloves = item;
                break;
            case ItemType.Boots:
                equippedBoots = item;
                break;
            case ItemType.Ring:
                equippedRing = item;
                break;
            case ItemType.Weapon:
                equippedWeapon = item;
                break;
            case ItemType.Rune:
                equippedRune = item;
                break;
            case ItemType.Book:
                equippedBook = item;
                break;
            default:
                return false;
        }

        CalculateTotalStats();
        SaveEquipmentToFirebase();
        return true;
    }

    /// <summary>
    /// Unequip an item from the appropriate slot
    /// </summary>
    public bool UnequipItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                equippedHelmet = null;
                break;
            case ItemType.Armor:
                equippedArmor = null;
                break;
            case ItemType.Gloves:
                equippedGloves = null;
                break;
            case ItemType.Boots:
                equippedBoots = null;
                break;
            case ItemType.Ring:
                equippedRing = null;
                break;
            case ItemType.Weapon:
                equippedWeapon = null;
                break;
            case ItemType.Rune:
                equippedRune = null;
                break;
            case ItemType.Book:
                equippedBook = null;
                break;
            default:
                return false;
        }

        CalculateTotalStats();
        SaveEquipmentToFirebase();
        return true;
    }

    /// <summary>
    /// Calculate total stats from all equipped items
    /// </summary>
    private void CalculateTotalStats()
    {
        var equippedItems = new List<InventoryItem>
        {
            equippedHelmet,
            equippedArmor,
            equippedGloves,
            equippedBoots,
            equippedRing,
            equippedWeapon,
            equippedRune,
            equippedBook
        }.Where(item => item != null).ToList();

        TotalDamageBonus = equippedItems.Sum(item => item.damageBonus);
        TotalSpeedBonus = equippedItems.Sum(item => item.speedBonus);
        TotalHealthBonus = equippedItems.Sum(item => item.healthBonus);
        
        Debug.Log($"Total Equipment Stats - Damage: {TotalDamageBonus}, Speed: {TotalSpeedBonus}, Health: {TotalHealthBonus}");
    }

    /// <summary>
    /// Get all currently equipped items
    /// </summary>
    public List<InventoryItem> GetEquippedItems()
    {
        return new List<InventoryItem>
        {
            equippedHelmet,
            equippedArmor,
            equippedGloves,
            equippedBoots,
            equippedRing,
            equippedWeapon,
            equippedRune,
            equippedBook
        }.Where(item => item != null).ToList();
    }

    /// <summary>
    /// Check if a specific item type is equipped
    /// </summary>
    public bool IsItemTypeEquipped(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Helmet:
                return equippedHelmet != null;
            case ItemType.Armor:
                return equippedArmor != null;
            case ItemType.Gloves:
                return equippedGloves != null;
            case ItemType.Boots:
                return equippedBoots != null;
            case ItemType.Ring:
                return equippedRing != null;
            case ItemType.Weapon:
                return equippedWeapon != null;
            case ItemType.Rune:
                return equippedRune != null;
            case ItemType.Book:
                return equippedBook != null;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Save current equipment to Firebase
    /// </summary>
    private async void SaveEquipmentToFirebase()
    {
        try
        {
            if (FirebaseDataManager.Instance != null)
            {
                var playerData = FirebaseDataManager.Instance.GetCurrentPlayerData();
                if (playerData != null)
                {
                    // Update equipped items in player data
                    playerData.inventoryData.equippedHelmetId = equippedHelmet?.itemId ?? "";
                    playerData.inventoryData.equippedArmorId = equippedArmor?.itemId ?? "";
                    playerData.inventoryData.equippedGlovesId = equippedGloves?.itemId ?? "";
                    playerData.inventoryData.equippedBootsId = equippedBoots?.itemId ?? "";
                    playerData.inventoryData.equippedRingId = equippedRing?.itemId ?? "";
                    playerData.inventoryData.equippedWeaponId = equippedWeapon?.itemId ?? "";
                    playerData.inventoryData.equippedRuneId = equippedRune?.itemId ?? "";
                    playerData.inventoryData.equippedBookId = equippedBook?.itemId ?? "";
                    
                    // Update total stats
                    playerData.totalDamageBonus = TotalDamageBonus;
                    playerData.totalSpeedBonus = TotalSpeedBonus;
                    playerData.totalHealthBonus = TotalHealthBonus;
                    playerData.health = playerData.baseHealth + TotalHealthBonus;
                    playerData.damage = playerData.baseDamage + TotalDamageBonus;
                    playerData.speed = playerData.baseSpeed + TotalSpeedBonus;

                    // Save to Firebase
                    await FirebaseDataManager.Instance.SavePlayerData(playerData);
                    Debug.Log("Equipment saved to Firebase successfully");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save equipment to Firebase: {e.Message}");
        }
    }

    /// <summary>
    /// Load equipment from Firebase data
    /// </summary>
    public void LoadEquipmentFromPlayerData(PlayerData playerData)
    {
        if (playerData == null) return;
        
        // Load equipped items by ID
        equippedHelmet = GetItemById(playerData.inventoryData.equippedHelmetId);
        equippedArmor = GetItemById(playerData.inventoryData.equippedArmorId);
        equippedGloves = GetItemById(playerData.inventoryData.equippedGlovesId);
        equippedBoots = GetItemById(playerData.inventoryData.equippedBootsId);
        equippedRing = GetItemById(playerData.inventoryData.equippedRingId);
        equippedWeapon = GetItemById(playerData.inventoryData.equippedWeaponId);
        equippedRune = GetItemById(playerData.inventoryData.equippedRuneId);
        equippedBook = GetItemById(playerData.inventoryData.equippedBookId);
        
        // Recalculate stats
        CalculateTotalStats();
        
        Debug.Log("Equipment loaded from Firebase data");
    }
    
    /// <summary>
    /// Get item by ID from inventory data
    /// </summary>
    private InventoryItem GetItemById(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return null;
        
        if (FirebaseDataManager.Instance != null)
        {
            var inventoryData = FirebaseDataManager.Instance.GetCurrentInventoryData();
            if (inventoryData != null)
            {
                // Find item in inventory by ID
                var itemData = inventoryData.items.FirstOrDefault(item => item.itemId == itemId);
                if (itemData != null)
                {
                    return itemData.ToInventoryItem();
                }
            }
        }
        
        return null;
    }
    
    private void InitData()
    {
        var playerData = FirebaseDataManager.Instance.GetCurrentPlayerData();
        if (playerData != null)
        {
            LoadEquipmentFromPlayerData(playerData);
        }
    }
}
