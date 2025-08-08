using UnityEngine;
using UnityEngine.UI;
using System.Reflection;

public class FireballCooldownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _cooldownImage;
    
    [Header("Settings")]
    [SerializeField] private bool _clockwise = true;
    [SerializeField] private float _cooldownDuration = 2f; // Thời gian cooldown 2 giây
    
    private PlayerController _playerController;
    private float _fireRate;
    private float _nextFireTime;
    private float _lastFireTime;
    private bool _isCooldownActive = false;
    
    private void Start()
    {
        // Find player controller
        _playerController = FindObjectOfType<PlayerController>();
        
        if (_playerController == null)
        {
            Debug.LogWarning("FireballCooldownUI: No PlayerController found!");
            return;
        }
        
        // Get private fields using reflection
        var fireRateField = typeof(PlayerController).GetField("_fireRate", BindingFlags.NonPublic | BindingFlags.Instance);
        var nextFireTimeField = typeof(PlayerController).GetField("_nextFireTime", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (fireRateField != null && nextFireTimeField != null)
        {
            _fireRate = (float)fireRateField.GetValue(_playerController);
            _nextFireTime = (float)nextFireTimeField.GetValue(_playerController);
        }
        
        // Setup image
        if (_cooldownImage != null)
        {
            _cooldownImage.type = Image.Type.Filled;
            _cooldownImage.fillMethod = Image.FillMethod.Radial360;
            _cooldownImage.fillOrigin = _clockwise ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
            _cooldownImage.fillAmount = 0f;
            _cooldownImage.gameObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (_playerController == null || _cooldownImage == null) return;
        
        // Get current values using reflection
        var fireRateField = typeof(PlayerController).GetField("_fireRate", BindingFlags.NonPublic | BindingFlags.Instance);
        var nextFireTimeField = typeof(PlayerController).GetField("_nextFireTime", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (fireRateField != null && nextFireTimeField != null)
        {
            float oldNextFireTime = _nextFireTime;
            _fireRate = (float)fireRateField.GetValue(_playerController);
            _nextFireTime = (float)nextFireTimeField.GetValue(_playerController);
            
            // Check if player just fired (nextFireTime increased)
            if (_nextFireTime > oldNextFireTime && !_isCooldownActive)
            {
                StartCooldown();
            }
        }
        
        // Update cooldown UI
        if (_isCooldownActive)
        {
            float currentTime = _playerController.Runner.SimulationTime;
            float elapsedTime = currentTime - _lastFireTime;
            float progress = 1f - (elapsedTime / _cooldownDuration);
            progress = Mathf.Clamp01(progress);
            
            _cooldownImage.gameObject.SetActive(true);
            _cooldownImage.fillAmount = progress;
            
            // Check if cooldown finished
            if (progress <= 0f)
            {
                _isCooldownActive = false;
                _cooldownImage.gameObject.SetActive(false);
            }
        }
    }
    
    private void StartCooldown()
    {
        _lastFireTime = _playerController.Runner.SimulationTime;
        _isCooldownActive = true;
        _cooldownImage.fillAmount = 1f; // Start at full
        _cooldownImage.gameObject.SetActive(true);
    }
} 