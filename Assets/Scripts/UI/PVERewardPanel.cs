using UnityEngine;
using TMPro;
public class PVERewardPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _xpText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _rubyText;
    public void SetData(int xpReward, int goldReward, int rubyReward)
    {
        _xpText.text = xpReward.ToString();
        _goldText.text = goldReward.ToString();
        _rubyText.text = rubyReward.ToString();
    }
}
