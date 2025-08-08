using TMPro;
using UnityEngine;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _oderText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _killText;
    [SerializeField] private TMP_Text _deathText;
    public void SetData(int oder, string playerName, int kills, int deaths)
    {
        _oderText.text = oder.ToString();
        _nameText.text = playerName;
        _killText.text = kills.ToString();
        _deathText.text = deaths.ToString();
    }
}
