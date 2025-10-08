using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Mission : MonoBehaviour
{
    [SerializeField] private TMP_Text _missionName;
    [SerializeField] private TMP_Text _missionDescription;
    [SerializeField] private Button _claimButton;
    [SerializeField] private Image _claimImage;
    private MissionReward _missionReward;

    public void SetMission(MissionReward missionReward)
    {
        _claimImage.gameObject.SetActive(false);
        _missionReward = missionReward;
        _missionName.text = missionReward.missionName;
        _missionDescription.text = missionReward.missionDescription;
        if(FirebaseDataManager.Instance.GetCurrentUserLevel() >= missionReward.levelRequirement)
        {
            _claimButton.interactable = true;
        }
        else
        {
            _claimButton.interactable = false;
        }
        if(FirebaseDataManager.Instance.GetCurrentPlayerData().IsMissionCompleted(missionReward.missionId)) {
            _claimButton.gameObject.SetActive(false);
            _claimImage.gameObject.SetActive(true);
        }
        _claimButton.onClick.AddListener(HandleClaimButtonClicked);
    }

    private void HandleClaimButtonClicked()
    {
        FirebaseDataManager.Instance.ClaimMissionReward(_missionReward);
        _claimButton.gameObject.SetActive(false);
        _claimImage.gameObject.SetActive(true);
        UIManager.Instance.ShowRewardPanel(_missionReward);
    }
}
