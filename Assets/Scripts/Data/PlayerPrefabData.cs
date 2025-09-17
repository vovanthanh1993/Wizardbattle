using UnityEngine;
using Fusion;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerPrefabData", menuName = "Game/Player Prefab Data")]
public class PlayerPrefabData : ScriptableObject
{
    [System.Serializable]
    public class PrefabMapping
    {
        public string characterName;
        public NetworkPrefabRef prefabRef;
    }
    
    [SerializeField] private PrefabMapping[] _prefabMappings;
    
    private Dictionary<string, NetworkPrefabRef> _prefabDictionary;
    
    public NetworkPrefabRef GetPrefabByName(string characterName)
    {
        if (_prefabDictionary == null)
        {
            InitializeDictionary();
        }
        
        if (_prefabDictionary.TryGetValue(characterName, out NetworkPrefabRef prefab))
        {
            return prefab;
        }
        
        Debug.LogWarning($"Character '{characterName}' not found in PlayerPrefabData");
        return _prefabMappings.Length > 0 ? _prefabMappings[0].prefabRef : default;
    }
    
    private void InitializeDictionary()
    {
        _prefabDictionary = new Dictionary<string, NetworkPrefabRef>();
        foreach (var mapping in _prefabMappings)
        {
            _prefabDictionary[mapping.characterName] = mapping.prefabRef;
        }
    }
}
