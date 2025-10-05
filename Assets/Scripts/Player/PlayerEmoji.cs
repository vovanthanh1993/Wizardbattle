using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Fusion;
public enum EmojiType
{
    Happy,
    Sad,
    Fight,
    Angry
}
public class PlayerEmoji : NetworkBehaviour
{
    [SerializeField] private Image _emojiImage;
    [SerializeField] private GameObject _emojiPivot;
    [SerializeField] private float _delay = 4f;
    
    private Coroutine _hideCoroutine;

    public override void Spawned() {
        _emojiPivot.gameObject.SetActive(false);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcShowEmoji(EmojiType emoji)
    {
        if (_hideCoroutine != null)
        {
            StopCoroutine(_hideCoroutine);
        }
        
        _emojiImage.sprite = GameCommonUtils.GetEmojiSprite(emoji);
        _emojiPivot.gameObject.SetActive(true);
        
        _hideCoroutine = StartCoroutine(HideEmojiAfterDelay(_delay));
    }
    
    private IEnumerator HideEmojiAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _emojiPivot.gameObject.SetActive(false);
        _hideCoroutine = null;
    }

    public void UpdateUIElements()
    {
        if (_emojiPivot != null && _emojiPivot.activeInHierarchy)
        {
            _emojiPivot.transform.LookAt(Camera.main.transform);
            _emojiPivot.transform.Rotate(0, 180, 0);
            Debug.Log("PlayerEmoji: UpdateUIElements - Emoji is active");
        }
    }
}
