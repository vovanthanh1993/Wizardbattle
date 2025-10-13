using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 1000f;
    [SerializeField] private float _currentHealth;

    [Header("UI References")]
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private GameObject _pivotHealthBar;
    
    [Header("Damage Text Settings")]
    [SerializeField] private float damageTextDuration = 1f;
    [SerializeField] private float damageTextFadeSpeed = 2f;
    
    // Track current damage text coroutine
    private Coroutine currentDamageTextCoroutine;
    
    [Header("Health Bar Settings")]
    [SerializeField] private float healthBarHideDelay = 5f;

    [SerializeField] private EnemyController _enemyController;
    
    private Coroutine _hideHealthBarCoroutine;

    private void Start()
    {
        Init();
    }

    public void Init() {
        _currentHealth = _maxHealth;
        UpdateHealthBar(_currentHealth, _maxHealth);
        if (_enemyController == null) {
            _enemyController = GetComponent<EnemyController>();
        }
        damageText.gameObject.SetActive(false);
        if (_pivotHealthBar != null) {
            _pivotHealthBar.SetActive(false);
        } else {
            UIManager.Instance.GamePlayPanel.ShowBossHealthBar(true);
        }
    }
    
    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (_healthBarImage != null)
        {
            float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
            _healthBarImage.fillAmount = fillAmount;
        } else {
            UIManager.Instance.GamePlayPanel.UpdateBossHealth(currentHealth, maxHealth);
        }
    }

    public void ShowDamageText(float damage)
    {
        // Stop previous coroutine if running
        if (currentDamageTextCoroutine != null)
        {
            StopCoroutine(currentDamageTextCoroutine);
        }
        
        // Set damage text
        damageText.text = damage.ToString();
        
        // Start new coroutine
        currentDamageTextCoroutine = StartCoroutine(ShowAndHideDamageText());
    }
        
    private IEnumerator ShowAndHideDamageText()
        {
        if (damageText == null) yield break;
        
        // Show damage text
        damageText.gameObject.SetActive(true);
        damageText.color = new Color(damageText.color.r, damageText.color.g, damageText.color.b, 1f);
        
        // Wait for duration
        yield return new WaitForSeconds(damageTextDuration);
        
        // Fade out damage text
        float fadeTime = 1f / damageTextFadeSpeed;
        float elapsedTime = 0f;
        Color startColor = damageText.color;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsedTime / fadeTime);
            damageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        // Hide damage text
        damageText.gameObject.SetActive(false);
        
        // Clear coroutine reference
        currentDamageTextCoroutine = null;
    }

    public virtual void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);
        UpdateHealthBar(_currentHealth, _maxHealth);
        
        // Show health bar when taking damage
        ShowHealthBar();
        
        // Show damage text
        ShowDamageText(damage);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        _currentHealth += healAmount;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth);
        UpdateHealthBar(_currentHealth, _maxHealth);
        
        // Show health bar when healing
        ShowHealthBar();
    }
    
    private void ShowHealthBar()
    {
        if (_pivotHealthBar != null)
        {
            _pivotHealthBar.SetActive(true);
            
            // Stop previous hide coroutine if running
            if (_hideHealthBarCoroutine != null)
            {
                StopCoroutine(_hideHealthBarCoroutine);
            }
            
            // Start new hide coroutine
            _hideHealthBarCoroutine = StartCoroutine(HideHealthBarAfterDelay());
        }
    }
    
    private IEnumerator HideHealthBarAfterDelay()
    {
        yield return new WaitForSeconds(healthBarHideDelay);
        
        if (_pivotHealthBar != null)
        {
            _pivotHealthBar.SetActive(false);
        }
        
        _hideHealthBarCoroutine = null;
    }

    public bool IsDead()
    {
        return _currentHealth <= 0;
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    public float GetMaxHealth()
    {
        return _maxHealth;
    }

    public float GetHealthPercentage()
    {
        return _currentHealth / _maxHealth;
    }

    private void Die()
    {
        _enemyController.Die();
        if (_pivotHealthBar != null) {
            _pivotHealthBar.SetActive(false);
        } else {
            UIManager.Instance.GamePlayPanel.ShowBossHealthBar(false);
        }
    }

    private void Update() {
        UpdateHealthBarBillboard();
    }

    public void UpdateHealthBarBillboard()
    {
        if (_pivotHealthBar != null && _pivotHealthBar.activeInHierarchy)
        {
            _pivotHealthBar.transform.LookAt(Camera.main.transform);
            _pivotHealthBar.transform.Rotate(0, 0, 0);
        }
    }
}
