using UnityEngine;

public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] private Transform _modelParent;
    [SerializeField] private GameObject[] _characterPrefabs;

    [SerializeField] private GameObject _content;

    private GameObject currentCharacter;
    private string currentCharacterName;
    
    public void ShowCharacter(string characterName)
    {
        if (_modelParent == null) _modelParent = GameObject.FindGameObjectWithTag("CharacterSelectorRoot").transform;
        if (_modelParent != null)
        {
            foreach (Transform child in _modelParent)
            {
                Destroy(child.gameObject);
            }
        }

        currentCharacterName = characterName;
        GameObject prefabToInstantiate = GetCharacterPrefabByName(characterName);
        
        if (prefabToInstantiate != null)
        {
            currentCharacter = Instantiate(prefabToInstantiate, _modelParent);
            currentCharacter.transform.localPosition = Vector3.zero;
            currentCharacter.transform.localRotation = Quaternion.identity;
        }
    }
    
    private GameObject GetCharacterPrefabByName(string characterName)
    {
        foreach (GameObject prefab in _characterPrefabs)
        {
            if (prefab.name == characterName)
                return prefab;
        }
        return null;
    }
    
    private int GetCharacterIndexByName(string characterName)
    {
        for (int i = 0; i < _characterPrefabs.Length; i++)
        {
            if (_characterPrefabs[i].name == characterName)
                return i;
        }
        return 0;
    }

    public void NextCharacter()
    {
        int currentIndex = GetCharacterIndexByName(currentCharacterName);
        int newIndex = (currentIndex + 1) % _characterPrefabs.Length;
        ShowCharacter(_characterPrefabs[newIndex].name);
    }

    public void PreviousCharacter()
    {
        int currentIndex = GetCharacterIndexByName(currentCharacterName);
        int newIndex = (currentIndex - 1 + _characterPrefabs.Length) % _characterPrefabs.Length;
        ShowCharacter(_characterPrefabs[newIndex].name);
    }

    public async void ConfirmSelection()
    {
        PlayerPrefs.SetString("SelectedCharacterName", currentCharacterName);
        PlayerPrefs.Save();
        FirebaseDataManager.Instance.GetCurrentPlayerData().playerPrefabName = currentCharacterName;
        
        // Save to Firebase
        UIManager.Instance.ShowLoadingPanel(true);
        await FirebaseDataManager.Instance.SavePlayerData(FirebaseDataManager.Instance.GetCurrentPlayerData());
        UIManager.Instance.ShowLoadingPanel(false);
    }

    private void Start() {
        ShowCharacterInMenu();
    }

    private void ShowCharacterInMenu() {
        currentCharacterName = FirebaseDataManager.Instance.GetCurrentPlayerData().playerPrefabName;
        ShowCharacter(currentCharacterName);
    }

    public void ClosePanel()
    {
        ShowCharacterInMenu();
        UIManager.Instance.ShowMenu();
        _content.gameObject.SetActive(false);
    }
}
