using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingPopup : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private Image _musicVolumeImageFill;
    [SerializeField] private Button _musicIncreaseButton;
    [SerializeField] private Button _musicDecreaseButton;
    [SerializeField] private Button _musicToggleOnButton;
    [SerializeField] private Button _musicToggleOffButton;

    [Header("SFX Settings")]
    [SerializeField] private Image _sfxVolumeImageFill;
    [SerializeField] private Button _sfxIncreaseButton;
    [SerializeField] private Button _sfxDecreaseButton;
    [SerializeField] private Button _sfxToggleOnButton;
    [SerializeField] private Button _sfxToggleOffButton;

    [Header("Mouse Settings")]
    [SerializeField] private Image _mouseSensitivityImageFill;
    [SerializeField] private Button _mouseIncreaseButton;
    [SerializeField] private Button _mouseDecreaseButton;

    [Header("Volume Settings")]
    [SerializeField] private float _volumeStep = 0.1f;
    [SerializeField] private float _mouseSensitivityStep = 0.1f;

    private void OnEnable()
    {
        UpdateUI();
        AudioManager.Instance.PlayButtonPopupSound();
    }

    public bool IsVisible()
    {
        return gameObject.activeSelf;
    }

    private void Start()
    {
        // Setup button listeners
        if (_musicIncreaseButton != null)
        {
            _musicIncreaseButton.onClick.AddListener(IncreaseMusicVolume);
        }

        if (_musicDecreaseButton != null)
        {
            _musicDecreaseButton.onClick.AddListener(DecreaseMusicVolume);
        }

        if (_musicToggleOnButton != null)
        {
            _musicToggleOnButton.onClick.AddListener(ToggleMusic);
        }

        if (_musicToggleOffButton != null)
        {
            _musicToggleOffButton.onClick.AddListener(ToggleMusic);
        }

        if (_sfxIncreaseButton != null)
        {
            _sfxIncreaseButton.onClick.AddListener(IncreaseSFXVolume);
        }

        if (_sfxDecreaseButton != null)
        {
            _sfxDecreaseButton.onClick.AddListener(DecreaseSFXVolume);
        }

        if (_sfxToggleOnButton != null)
        {
            _sfxToggleOnButton.onClick.AddListener(ToggleSFX);
        }

        if (_sfxToggleOffButton != null)
        {
            _sfxToggleOffButton.onClick.AddListener(ToggleSFX);
        }

        if (_mouseIncreaseButton != null)
        {
            _mouseIncreaseButton.onClick.AddListener(IncreaseMouseSensitivity);
        }

        if (_mouseDecreaseButton != null)
        {
            _mouseDecreaseButton.onClick.AddListener(DecreaseMouseSensitivity);
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (AudioManager.Instance == null) return;

        // Update music volume fill
        if (_musicVolumeImageFill != null)
        {
            _musicVolumeImageFill.fillAmount = AudioManager.Instance.GetMusicVolume();
        }

        // Update SFX volume fill
        if (_sfxVolumeImageFill != null)
        {
            _sfxVolumeImageFill.fillAmount = AudioManager.Instance.GetSFXVolume();
        }

        // Update music toggle buttons
        bool musicEnabled = AudioManager.Instance.IsMusicEnabled();
        if (_musicToggleOnButton != null)
        {
            _musicToggleOnButton.gameObject.SetActive(musicEnabled);
        }
        if (_musicToggleOffButton != null)
        {
            _musicToggleOffButton.gameObject.SetActive(!musicEnabled);
        }

        // Update SFX toggle buttons
        bool sfxEnabled = AudioManager.Instance.IsSFXEnabled();
        if (_sfxToggleOnButton != null)
        {
            _sfxToggleOnButton.gameObject.SetActive(sfxEnabled);
        }
        if (_sfxToggleOffButton != null)
        {
            _sfxToggleOffButton.gameObject.SetActive(!sfxEnabled);
        }

        // Update mouse sensitivity fill
        if (_mouseSensitivityImageFill != null)
        {
            _mouseSensitivityImageFill.fillAmount = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        }
    }

    public void IncreaseMusicVolume()
    {
        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.GetMusicVolume();
            float newVolume = Mathf.Min(currentVolume + _volumeStep, 1f);
            AudioManager.Instance.SetMusicVolume(newVolume);
            UpdateUI();
            AudioManager.Instance.PlayButtonChangeSound();
        }
    }

    public void DecreaseMusicVolume()
    {
        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.GetMusicVolume();
            float newVolume = Mathf.Max(currentVolume - _volumeStep, 0.1f);
            AudioManager.Instance.SetMusicVolume(newVolume);
            UpdateUI();
            AudioManager.Instance.PlayButtonChangeSound();
        }
    }

    public void ToggleMusic()
    {
        if (AudioManager.Instance != null)
        {
            bool currentState = AudioManager.Instance.IsMusicEnabled();
            AudioManager.Instance.ToggleMusic(!currentState);
            UpdateUI();
            AudioManager.Instance.PlayButtonChangeSound();
        }
    }

    public void IncreaseSFXVolume()
    {
        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.GetSFXVolume();
            float newVolume = Mathf.Min(currentVolume + _volumeStep, 1f);
            AudioManager.Instance.SetSFXVolume(newVolume);
            UpdateUI();
            AudioManager.Instance.PlayButtonChangeSound();
        }
    }

    public void DecreaseSFXVolume()
    {
        if (AudioManager.Instance != null)
        {
            float currentVolume = AudioManager.Instance.GetSFXVolume();
            float newVolume = Mathf.Max(currentVolume - _volumeStep, 0.1f);
            AudioManager.Instance.SetSFXVolume(newVolume);
            UpdateUI();
            AudioManager.Instance.PlayButtonChangeSound();
        }
    }

    public void ToggleSFX()
    {
        if (AudioManager.Instance != null)
        {
            bool currentState = AudioManager.Instance.IsSFXEnabled();
            AudioManager.Instance.ToggleSFX(!currentState);
            UpdateUI();
            if (!currentState) // If SFX is enabled
            {
                AudioManager.Instance.PlayButtonChangeSound();
            }
        }
    }

    public void IncreaseMouseSensitivity()
    {
        float currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        float newSensitivity = Mathf.Min(currentSensitivity + _mouseSensitivityStep, 2f);
        PlayerPrefs.SetFloat("MouseSensitivity", newSensitivity);
        PlayerPrefs.Save();
        UpdateUI();
        AudioManager.Instance.PlayButtonChangeSound();
    }

    public void DecreaseMouseSensitivity()
    {
        float currentSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        float newSensitivity = Mathf.Max(currentSensitivity - _mouseSensitivityStep, 0.1f);
        PlayerPrefs.SetFloat("MouseSensitivity", newSensitivity);
        PlayerPrefs.Save();
        UpdateUI();
        AudioManager.Instance.PlayButtonChangeSound();
    }

    public void OnCloseButton()
    {
        AudioManager.Instance.PlayButtonCloseSound();
    }
}
