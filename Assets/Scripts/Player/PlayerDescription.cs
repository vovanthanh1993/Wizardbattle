using UnityEngine;
using TMPro;
public class PlayerDescription : MonoBehaviour
{
    [SerializeField] private string _characterName;
    [SerializeField] private string _characterDescription;

    public string GetCharacterName()
    {
        return _characterName;
    }

    public string GetCharacterDescription()
    {
        return _characterDescription;
    }
}
