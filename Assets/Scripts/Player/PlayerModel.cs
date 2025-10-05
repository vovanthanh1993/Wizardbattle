using UnityEngine;
using Fusion;

public class PlayerModel : NetworkBehaviour
{
    [SerializeField] private GameObject _weaponHolder       ;
    [SerializeField] private GameObject _amuletHolder;
    [SerializeField] private GameObject _ringHolder;
    [SerializeField] private GameObject _glovesHolder;
    [SerializeField] private GameObject _bootsHolder;
    [SerializeField] private GameObject _helmetHolder;
    [SerializeField] private GameObject _armorHolder;
    [SerializeField] private GameObject _bookHolder;

    [Networked] public string WeaponMaterialTier { get; set; }
    [Networked] public string ArmorMaterialTier { get; set; }
    [Networked] public string GlovesMaterialTier { get; set; }
    [Networked] public string BootsMaterialTier { get; set; }
    [Networked] public string HelmetMaterialTier { get; set; }
    [Networked] public string RingMaterialTier { get; set; }
    [Networked] public string AmuletMaterialTier { get; set; }
    [Networked] public string BookMaterialTier { get; set; }

    public void SetMaterialTier() {
        PlayerData playerData = FirebaseDataManager.Instance.GetCurrentPlayerData();
        string weaponMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedWeaponId);
        string armorMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedArmorId);
        string glovesMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedGlovesId);
        string bootsMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedBootsId);
        string helmetMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedHelmetId);
        string ringMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedRingId);
        string amuletMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedAmuletId);
        string bookMaterialTier = GetMaterialTier(playerData.inventoryData, playerData.inventoryData.equippedBookId);
        if (Object != null) RpcSetData(weaponMaterialTier, armorMaterialTier, glovesMaterialTier, bootsMaterialTier, helmetMaterialTier, ringMaterialTier, amuletMaterialTier, bookMaterialTier);
        else SetModel(weaponMaterialTier, armorMaterialTier, glovesMaterialTier, bootsMaterialTier, helmetMaterialTier, ringMaterialTier, amuletMaterialTier, bookMaterialTier);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcSetData(string weaponMaterialTier, string armorMaterialTier, string glovesMaterialTier, string bootsMaterialTier, string helmetMaterialTier, string ringMaterialTier, string amuletMaterialTier, string bookMaterialTier) {
        WeaponMaterialTier = weaponMaterialTier;
        ArmorMaterialTier = armorMaterialTier;
        GlovesMaterialTier = glovesMaterialTier;
        BootsMaterialTier = bootsMaterialTier;
        HelmetMaterialTier = helmetMaterialTier;
        RingMaterialTier = ringMaterialTier;
        AmuletMaterialTier = amuletMaterialTier;
        BookMaterialTier = bookMaterialTier;
        SetModel();
    }
    public void SetModel()
    {    
        // Set weapon
        if (!string.IsNullOrEmpty(WeaponMaterialTier) && _weaponHolder != null)
        {
            ClearItemContent(_weaponHolder);
            _weaponHolder.SetActive(true);
            GameObject weaponPrefab = GameCommonUtils.GetItemPrefab(ItemType.Weapon, WeaponMaterialTier);
            if (weaponPrefab != null)
            {
                Instantiate(weaponPrefab, _weaponHolder.transform);
            }
        }
        
        // Set armor
        if (!string.IsNullOrEmpty(ArmorMaterialTier) && _armorHolder != null)
        {
            ClearItemContent(_armorHolder);
            _armorHolder.SetActive(true);
            GameObject armorPrefab = GameCommonUtils.GetItemPrefab(ItemType.Armor, ArmorMaterialTier);
            if (armorPrefab != null)
            {
                Instantiate(armorPrefab, _armorHolder.transform);
            }
        }
        
        // Set gloves
        if (!string.IsNullOrEmpty(GlovesMaterialTier) && _glovesHolder != null)
        {
            ClearItemContent(_glovesHolder);
            _glovesHolder.SetActive(true);
            GameObject glovesPrefab = GameCommonUtils.GetItemPrefab(ItemType.Gloves, GlovesMaterialTier);
            if (glovesPrefab != null)
            {
                Instantiate(glovesPrefab, _glovesHolder.transform);
            }
        }
        
        // Set boots
        if (!string.IsNullOrEmpty(BootsMaterialTier) && _bootsHolder != null)
        {
            ClearItemContent(_bootsHolder);
            _bootsHolder.SetActive(true);
            GameObject bootsPrefab = GameCommonUtils.GetItemPrefab(ItemType.Boots, BootsMaterialTier);
            if (bootsPrefab != null)
            {
                Instantiate(bootsPrefab, _bootsHolder.transform);
            }
        }
        
        // Set helmet
        if (!string.IsNullOrEmpty(HelmetMaterialTier) && _helmetHolder != null)
        {
            ClearItemContent(_helmetHolder);
            _helmetHolder.SetActive(true);
            GameObject helmetPrefab = GameCommonUtils.GetItemPrefab(ItemType.Helmet, HelmetMaterialTier);
            if (helmetPrefab != null)
            {
                Instantiate(helmetPrefab, _helmetHolder.transform);
            }
        }
        
        // Set ring
        if (!string.IsNullOrEmpty(RingMaterialTier) && _ringHolder != null)
        {
            ClearItemContent(_ringHolder);
            _ringHolder.SetActive(true);
            GameObject ringPrefab = GameCommonUtils.GetItemPrefab(ItemType.Ring, RingMaterialTier);
            if (ringPrefab != null)
            {
                Instantiate(ringPrefab, _ringHolder.transform);
            }
        }
        
        // Set amulet
        if (!string.IsNullOrEmpty(AmuletMaterialTier) && _amuletHolder != null)
        {
            ClearItemContent(_amuletHolder);
            _amuletHolder.SetActive(true);
            GameObject amuletPrefab = GameCommonUtils.GetItemPrefab(ItemType.Amulet, AmuletMaterialTier);
            if (amuletPrefab != null)
            {
                Instantiate(amuletPrefab, _amuletHolder.transform);
            }
        }
        
        // Set book
        if (!string.IsNullOrEmpty(BookMaterialTier) && _bookHolder != null)
        {
            ClearItemContent(_bookHolder);
            _bookHolder.SetActive(true);
            GameObject bookPrefab = GameCommonUtils.GetItemPrefab(ItemType.Book, BookMaterialTier);
            if (bookPrefab != null)
            {
                Instantiate(bookPrefab, _bookHolder.transform);
            }
        }
    }

    public void SetModel(string weaponMaterialTier, string armorMaterialTier, string glovesMaterialTier, string bootsMaterialTier, string helmetMaterialTier, string ringMaterialTier, string amuletMaterialTier, string bookMaterialTier)
    {   
        // Set weapon
        if (!string.IsNullOrEmpty(weaponMaterialTier) && _weaponHolder != null)
        {
            ClearItemContent(_weaponHolder);
            _weaponHolder.SetActive(true);
            GameObject weaponPrefab = GameCommonUtils.GetItemPrefab(ItemType.Weapon, weaponMaterialTier);
            if (weaponPrefab != null)
            {
                Instantiate(weaponPrefab, _weaponHolder.transform);
            }
        }
        
        // Set armor
        if (!string.IsNullOrEmpty(armorMaterialTier) && _armorHolder != null)
        {
            ClearItemContent(_armorHolder);
            _armorHolder.SetActive(true);
            GameObject armorPrefab = GameCommonUtils.GetItemPrefab(ItemType.Armor, armorMaterialTier);
            if (armorPrefab != null)
            {
                Instantiate(armorPrefab, _armorHolder.transform);
            }
        }
        
        // Set gloves
        if (!string.IsNullOrEmpty(glovesMaterialTier) && _glovesHolder != null)
        {
            ClearItemContent(_glovesHolder);
            _glovesHolder.SetActive(true);
            GameObject glovesPrefab = GameCommonUtils.GetItemPrefab(ItemType.Gloves, glovesMaterialTier);
            if (glovesPrefab != null)
            {
                Instantiate(glovesPrefab, _glovesHolder.transform);
            }
        }
        
        // Set boots
        if (!string.IsNullOrEmpty(bootsMaterialTier) && _bootsHolder != null)
        {
            ClearItemContent(_bootsHolder);
            _bootsHolder.SetActive(true);
            GameObject bootsPrefab = GameCommonUtils.GetItemPrefab(ItemType.Boots, bootsMaterialTier);
            if (bootsPrefab != null)
            {
                Instantiate(bootsPrefab, _bootsHolder.transform);
            }
        }
        
        // Set helmet
        if (!string.IsNullOrEmpty(helmetMaterialTier) && _helmetHolder != null)
        {
            ClearItemContent(_helmetHolder);
            _helmetHolder.SetActive(true);
            GameObject helmetPrefab = GameCommonUtils.GetItemPrefab(ItemType.Helmet, helmetMaterialTier);
            if (helmetPrefab != null)
            {
                Instantiate(helmetPrefab, _helmetHolder.transform);
            }
        }
        
        // Set ring
        if (!string.IsNullOrEmpty(ringMaterialTier) && _ringHolder != null)
        {
            ClearItemContent(_ringHolder);
            _ringHolder.SetActive(true);
            GameObject ringPrefab = GameCommonUtils.GetItemPrefab(ItemType.Ring, ringMaterialTier);
            if (ringPrefab != null)
            {
                Instantiate(ringPrefab, _ringHolder.transform);
            }
        }
        
        // Set amulet
        if (!string.IsNullOrEmpty(amuletMaterialTier) && _amuletHolder != null)
        {
            ClearItemContent(_amuletHolder);
            _amuletHolder.SetActive(true);
            GameObject amuletPrefab = GameCommonUtils.GetItemPrefab(ItemType.Amulet, amuletMaterialTier);
            if (amuletPrefab != null)
            {
                Instantiate(amuletPrefab, _amuletHolder.transform);
            }
        }
        
        // Set book
        if (!string.IsNullOrEmpty(bookMaterialTier) && _bookHolder != null)
        {
            ClearItemContent(_bookHolder);
            _bookHolder.SetActive(true);
            GameObject bookPrefab = GameCommonUtils.GetItemPrefab(ItemType.Book, bookMaterialTier);
            if (bookPrefab != null)
            {
                Instantiate(bookPrefab, _bookHolder.transform);
            }
        }
    }
    
    private string GetMaterialTier(InventoryData inventoryData, string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return string.Empty;
        
        var itemData = inventoryData.items.Find(x => x.itemId == itemId);
        return itemData?.ToInventoryItem().materialTier.ToString() ?? string.Empty;
    }

    private void Start()
    { 
        if(Object == null) {
            SetMaterialTier(); 
            SubscribeToEvents();
            Debug.Log("SetModel 1");
        }
    }

    public override void Spawned() {
        if(Object.HasInputAuthority) {
            SetMaterialTier();    
        }
        Debug.Log("SetModel 2");
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        EquipmentManager.OnItemEquipped += OnItemEquipped;
        EquipmentManager.OnItemUnequipped += OnItemUnequipped;
        EquipmentManager.OnEquipmentChanged += OnEquipmentChanged;
    }
    
    private void UnsubscribeFromEvents()
    {
        EquipmentManager.OnItemEquipped -= OnItemEquipped;
        EquipmentManager.OnItemUnequipped -= OnItemUnequipped;
        EquipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
    }
    
    private void OnItemEquipped(InventoryItem item)
    {
        Debug.Log($"Item equipped: {item.itemName}");
        // Có thể thêm logic specific cho từng item type nếu cần
    }
    
    private void OnItemUnequipped(ItemType itemType)
    {
        Debug.Log($"Item unequipped: {itemType}");
        // Có thể thêm logic specific cho từng item type nếu cần
    }
    
    private void OnEquipmentChanged()
    {
        SetMaterialTier();
    }

    private void ClearItemContent(GameObject parentObject)
    {
        // Clear all child objects in _itemContent
        foreach (Transform child in parentObject.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
