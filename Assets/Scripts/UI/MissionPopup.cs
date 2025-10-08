using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class MissionPopup : MonoBehaviour
{
    [SerializeField] private GameObject _content;
    [SerializeField] private GameObject _missionPrefab;

    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _prevButton;
    [SerializeField] private TMP_Text _pageText;
    
    private int currentPage = 0;
    private int missionsPerPage = 4;
    private List<MissionReward> allMissions;
    private void OnEnable() {
        LoadAllMissions();
        SetupButtons();
        ShowCurrentPage();
    }
    
    private void LoadAllMissions()
    {
        allMissions = FirebaseDataManager.Instance.GetCurrentGameData().missionRewards;
        Debug.Log("Total MissionRewards: " + allMissions.Count);
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
        int maxPages = Mathf.CeilToInt((float)allMissions.Count / missionsPerPage);
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
        int startIndex = currentPage * missionsPerPage;
        int endIndex = Mathf.Min(startIndex + missionsPerPage, allMissions.Count);
        
        // Show missions for current page
        for (int i = startIndex; i < endIndex; i++)
        {
            GameObject mission = Instantiate(_missionPrefab, _content.transform);
            mission.GetComponent<Mission>().SetMission(allMissions[i]);
        }
        
        // Update page text
        if (_pageText != null)
        {
            int maxPages = Mathf.CeilToInt((float)allMissions.Count / missionsPerPage);
            _pageText.text = $"Page {currentPage + 1} / {maxPages}";
        }
        
        // Update button states
        UpdateButtonStates();
        
        Debug.Log($"Showing missions {startIndex + 1}-{endIndex} of {allMissions.Count}");
    }
    
    private void UpdateButtonStates()
    {
        int maxPages = Mathf.CeilToInt((float)allMissions.Count / missionsPerPage);
        
        if (_prevButton != null)
            _prevButton.interactable = currentPage > 0; 
        if (_nextButton != null)
            _nextButton.interactable = currentPage < maxPages - 1;
    }
}
