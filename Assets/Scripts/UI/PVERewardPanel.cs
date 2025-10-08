using UnityEngine;
using TMPro;
public class PVERewardPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text _xpText;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _rubyText;
    [SerializeField] private TMP_Text _foodText;
    public void SetData(int xpReward, int goldReward, int rubyReward, int foodReward)
    {
        if(xpReward == 0) {
            _xpText.gameObject.transform.parent.gameObject.SetActive(false);
        } else {
            _xpText.gameObject.transform.parent.gameObject.SetActive(true);
            _xpText.text = xpReward.ToString();
        }
        if(goldReward == 0) {
            _goldText.gameObject.transform.parent.gameObject.SetActive(false);
        } else {
            _goldText.gameObject.transform.parent.gameObject.SetActive(true);
            _goldText.text = goldReward.ToString();
        }
        if(rubyReward == 0) {
            _rubyText.gameObject.transform.parent.gameObject.SetActive(false);
        } else {
            _rubyText.gameObject.transform.parent.gameObject.SetActive(true);
            _rubyText.text = rubyReward.ToString();
        }
        if(foodReward == 0) {
            _foodText.gameObject.transform.parent.gameObject.SetActive(false);
        } else {
            _foodText.gameObject.transform.parent.gameObject.SetActive(true);
            _foodText.text = foodReward.ToString();
        }
    }
}
