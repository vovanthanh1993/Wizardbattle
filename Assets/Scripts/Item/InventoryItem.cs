
using UnityEngine;

public enum ItemType
{
    Helmet,
    Armor,
    Gloves,
    Boots,
    Ring,
    Weapon,
    Amulet,
    Book

}

public enum MaterialTier {
    Bronze,
    Iron,
    Silver,
    Gold,
    Mythril

}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Basic Info")]

    public string itemId;
    public Rarity rarity;
    public ItemType itemType;
    public MaterialTier materialTier;
    public string itemName;
    public Sprite icon;
    public GameObject prefab;

    [Header("Stacking")]
    public bool isStackable;
    public int maxStack = 1;

    [Header("Item Stats")]
    [Tooltip("Damage bonus this item provides")]
    public int damageBonus = 0;
    
    [Tooltip("Speed bonus this item provides")]
    public int speedBonus = 0;
    
    [Tooltip("Health bonus this item provides")]
    public int healthBonus = 0;
}
