using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerLobbyInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private Image _avatarImage;

    public void SetData(string playerName, string prefabName)
    {
        _playerNameText.text = playerName;
        _avatarImage.sprite = GameCommonUtils.GetAvatarSprite(prefabName);
    }
}
