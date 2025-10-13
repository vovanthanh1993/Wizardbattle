using UnityEngine;
using TMPro;
using System.Collections;

public class PVPStartGame : MonoBehaviour
{
    [SerializeField] private TMP_Text _startGameText;
    
    [Header("Animation Settings")]
    [SerializeField] private float showDelay = 1f;    // Show after 1 second
    [SerializeField] private float hideDelay = 3f;    // Hide after 3 seconds
    [SerializeField] private float fadeSpeed = 2f;    // Fade speed

    void Start()
    {
        // Hide text initially
        if (_startGameText != null)
        {
            _startGameText.gameObject.SetActive(false);
            _startGameText.color = new Color(_startGameText.color.r, _startGameText.color.g, _startGameText.color.b, 0f);
        }
        
        // Start animation sequence
        StartCoroutine(StartGameAnimation());
    }
    
    private IEnumerator StartGameAnimation()
    {
        // Wait 1 second before showing
        yield return new WaitForSeconds(showDelay);
        
        // Show text
        if (_startGameText != null)
        {
            _startGameText.gameObject.SetActive(true);
            
            // Fade in
            yield return StartCoroutine(FadeIn());
            
            // Wait 3 seconds
            yield return new WaitForSeconds(hideDelay);
            
            // Fade out
            yield return StartCoroutine(FadeOut());
            
            // Hide text
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
