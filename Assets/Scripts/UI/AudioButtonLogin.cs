using UnityEngine;
using UnityEngine.UI;
public class AudioButtonLogin : MonoBehaviour
{
    [SerializeField] private Button _musicButtonOn;
    [SerializeField] private Button _musicButtonOff;
    [SerializeField] private Button _sfxButtonOn;
    [SerializeField] private Button _sfxButtonOff;

    private void Start()
    {
        _musicButtonOn.onClick.AddListener(ToggleMusic);
        _musicButtonOff.onClick.AddListener(ToggleMusic);
        _sfxButtonOn.onClick.AddListener(ToggleSFX);
        _sfxButtonOff.onClick.AddListener(ToggleSFX);
        UpdateUI();
    }

    public void ToggleMusic()
    {
        if (AudioManager.Instance != null)
        {
            bool currentState = AudioManager.Instance.IsMusicEnabled();
            AudioManager.Instance.ToggleMusic(!currentState);
            //AudioManager.Instance.PlayButtonChangeSound();
        }
    }

    public void ToggleSFX()
    {
        if (AudioManager.Instance != null)
        {
            bool currentState = AudioManager.Instance.IsSFXEnabled();
            AudioManager.Instance.ToggleSFX(!currentState);
            //AudioManager.Instance.PlayButtonChangeSound();
        }
    }

    private void OnEnable() {
        UpdateUI();
    }

    private void UpdateUI() {
        if (AudioManager.Instance != null) {
            _musicButtonOn.gameObject.SetActive(AudioManager.Instance.IsMusicEnabled());
            _musicButtonOff.gameObject.SetActive(!AudioManager.Instance.IsMusicEnabled());
            _sfxButtonOn.gameObject.SetActive(AudioManager.Instance.IsSFXEnabled());
            _sfxButtonOff.gameObject.SetActive(!AudioManager.Instance.IsSFXEnabled());
        }
    }
}
