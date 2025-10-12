using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
public class CoinShopManager : MonoBehaviour
{
    [SerializeField] private ShopType _shopType;
    [SerializeField] private GameObject _content;
    [SerializeField] private GameObject _shopPackagePrefab;

    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _prevButton;
    [SerializeField] private TMP_Text _pageText;
    [SerializeField] private TMP_Text _numberText;
    
    private int currentPage = 0;
    private int shopPackagesPerPage = 3;
    private List<ShopData> allShopPackages;
    private void OnEnable() {
        LoadAllShopPackages();
        SetupButtons();
        ShowCurrentPage();
        
        // Subscribe to events
        GameEvents.OnUpdateShopNumbers += UpdateNumber;
        UpdateNumber();
        AudioManager.Instance.PlayButtonPopupSound();
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        GameEvents.OnUpdateShopNumbers -= UpdateNumber;
    }
    
    private void LoadAllShopPackages()
    {
        allShopPackages = FirebaseDataManager.Instance.GetCurrentGameData().shopData.FindAll(shop => shop.buyType == _shopType);
        Debug.Log("Total packages: " + allShopPackages.Count);
    }
    
    private void SetupButtons()
    {
        if (_nextButton != null)
            _nextButton.onClick.AddListener(NextPage);
        if (_prevButton != null)
            _prevButton.onClick.AddListener(PrevPage);
    }
    
    private void NextPage()
    {
        int maxPages = Mathf.CeilToInt((float)allShopPackages.Count / shopPackagesPerPage);
        if (currentPage < maxPages - 1)
        {
            currentPage++;
            ShowCurrentPage();
        }
    }
    
    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowCurrentPage();
        }
    }
    
    private void ShowCurrentPage()
    {
        // Clear existing missions
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Calculate start and end index for current page
        int startIndex = currentPage * shopPackagesPerPage;
        int endIndex = Mathf.Min(startIndex + shopPackagesPerPage, allShopPackages.Count);
        
        // Show missions for current page
        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject mission = Instantiate(_shopPackagePrefab, _content.transform);
            mission.GetComponent<ShopPackageUI>().SetShopPackage(allShopPackages[i]);
        }
        
        // Update page text
        if (_pageText != null)
        {
            int maxPages = Mathf.CeilToInt((float)allShopPackages.Count / shopPackagesPerPage);
            _pageText.text = $"Page {currentPage + 1} / {maxPages}";
        }
        
        // Update button states
        UpdateButtonStates();
        
        Debug.Log($"Showing packages {startIndex + 1}-{endIndex} of {allShopPackages.Count}");
    }
    
    private void UpdateButtonStates()
    {
        int maxPages = Mathf.CeilToInt((float)allShopPackages.Count / shopPackagesPerPage);
        
        if (_prevButton != null)
            _prevButton.interactable = currentPage > 0; 
        if (_nextButton != null)
            _nextButton.interactable = currentPage < maxPages - 1;
    }

    public void UpdateNumber()
    {
        switch (_shopType)
        {
            case ShopType.Ruby:
                if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserRuby().ToString();
                break;
            case ShopType.Gold:
                if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserGold().ToString();
                break;
            case ShopType.Food:
                if (_numberText != null) _numberText.text = FirebaseDataManager.Instance.GetCurrentUserFood().ToString();
                break;
        }
    }
}
