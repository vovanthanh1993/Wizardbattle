using UnityEngine;
using TMPro;

public class NoticePopup : MonoBehaviour
{
    [SerializeField] private TMP_Text _noticeText;

    public void ShowNoticePopup(string text) {
        _noticeText.text = text;
        gameObject.SetActive(true);
    }
}
