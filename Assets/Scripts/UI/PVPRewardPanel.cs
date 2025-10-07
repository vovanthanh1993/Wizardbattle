using UnityEngine;
using TMPro;

public class PVPRewardPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _xpText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _rubyText;

    public void SetData(int rank, int xpReward, int goldReward, int rubyReward)
    {
        _rankText.text = "TOP " + rank;
        _xpText.text = xpReward.ToString();
        _goldText.text = goldReward.ToString();
        _rubyText.text = rubyReward.ToString();
    }
}
