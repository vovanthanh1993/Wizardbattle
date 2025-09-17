using UnityEngine;
using TMPro;
public class PlayerLobbyInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;

    public void SetData(string playerName)
    {
        _playerNameText.text = playerName;
    }
}
