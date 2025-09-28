using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class ItemPanel : MonoBehaviour
{
    [SerializeField] private GameObject _itemContent;
    [SerializeField] private GameObject _itemSlotPrefab;
    [SerializeField] private TMP_Text _titleText;

    private void OnEnable() {
        ClearItemContent();
    }
    public void ShowItemPanel(ItemType itemType)
    {
        StartCoroutine(ShowItemPanelCoroutine(itemType));
    }
    
    private IEnumerator ShowItemPanelCoroutine(ItemType itemType)
    {
        ClearItemContent();
        _titleText.text = itemType.ToString();
        // Wait for one frame to ensure clearing is complete
        yield return null;
        
        // Get list of items by ItemType from PlayerData
        List<InventoryItem> items = GetItemsByType(itemType);
        
        // Create ItemSlot for each item
        foreach (InventoryItem item in items)
        {
            CreateItemSlot(item);
        }
    }
    
    private List<InventoryItem> GetItemsByType(ItemType itemType)
    {
        // Get data directly from PlayerData through FirebaseDataManager
        if (FirebaseDataManager.Instance != null)
        {
            InventoryData inventoryData = FirebaseDataManager.Instance.GetCurrentInventoryData();
            if (inventoryData != null)
            {
                return inventoryData.GetItemsByType(itemType);
            }
        }
        
        Debug.LogWarning("FirebaseDataManager or InventoryData not found!");
        return new List<InventoryItem>();
    }
    
    private void CreateItemSlot(InventoryItem item)
    {
        // Create instance of ItemSlot prefab
        GameObject itemSlotObj = Instantiate(_itemSlotPrefab, _itemContent.transform);
        
        // Set data for ItemSlot
        ItemSlot itemSlot = itemSlotObj.GetComponent<ItemSlot>();
        if (itemSlot != null)
        {
            itemSlot.SetData(item);
        }
    }

    private void ClearItemContent()
    {
        // Clear all child objects in _itemContent
        foreach (Transform child in _itemContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
