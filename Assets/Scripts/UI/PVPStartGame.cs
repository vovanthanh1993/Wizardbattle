using UnityEngine;
using TMPro;
using System.Collections;

public class PVPStartGame : MonoBehaviour
{
    [SerializeField] private TMP_Text _startGameText;
    
    [Header("Animation Settings")]
    [SerializeField] private float showDelay = 1f;    // Hiện sau 1 giây
    [SerializeField] private float hideDelay = 3f;    // Ẩn sau 3 giây
    [SerializeField] private float fadeSpeed = 2f;    // Tốc độ fade

    void Start()
    {
        // Ẩn text ban đầu
        if (_startGameText != null)
        {
            _startGameText.gameObject.SetActive(false);
            _startGameText.color = new Color(_startGameText.color.r, _startGameText.color.g, _startGameText.color.b, 0f);
        }
        
        // Bắt đầu animation sequence
        StartCoroutine(StartGameAnimation());
    }
    
    private IEnumerator StartGameAnimation()
    {
        // Đợi 1 giây trước khi hiện
        yield return new WaitForSeconds(showDelay);
        
        // Hiện text
        if (_startGameText != null)
        {
            _startGameText.gameObject.SetActive(true);
            
            // Fade in
            yield return StartCoroutine(FadeIn());
            
            // Đợi 3 giây
            yield return new WaitForSeconds(hideDelay);
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Ẩn text
            _startGameText.gameObject.SetActive(false);
        }
    }
    
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color startColor = _startGameText.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
        
        while (elapsedTime < 1f / fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime * fadeSpeed);
            _startGameText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        _startGameText.color = targetColor;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color startColor = _startGameText.color;
        
        while (elapsedTime < 1f / fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime * fadeSpeed);
            _startGameText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        _startGameText.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
}
