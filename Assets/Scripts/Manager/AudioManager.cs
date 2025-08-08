using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip _mainMenuMusic;
    [SerializeField] private AudioClip _gameplayMusic;
    [SerializeField] private AudioClip _victoryMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip _healthPickupSound;
    [SerializeField] private AudioClip _fireballSound;
    [SerializeField] private AudioClip _explosionSound;
    [SerializeField] private AudioClip _playerHitSound;
    [SerializeField] private AudioClip _buttonClickSound;

    [Header("Audio Settings")]
    [SerializeField] private float _musicVolume = 0.7f;
    [SerializeField] private float _sfxVolume = 1f;
    [SerializeField] private bool _musicEnabled = true;
    [SerializeField] private bool _sfxEnabled = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeAudioSources();
    }

    private void Start()
    {
        PlayMainMenuMusic();
    }

    private void InitializeAudioSources()
    {
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
        }

        if (_sfxSource == null)
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.playOnAwake = false;
        }

        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        if (_musicSource != null)
        {
            _musicSource.volume = _musicEnabled ? _musicVolume : 0f;
        }

        if (_sfxSource != null)
        {
            _sfxSource.volume = _sfxEnabled ? _sfxVolume : 0f;
        }
    }

    #region Background Music
    public void PlayMainMenuMusic()
    {
        PlayMusic(_mainMenuMusic);
    }

    public void PlayGameplayMusic()
    {
        PlayMusic(_gameplayMusic);
    }

    public void PlayVictoryMusic()
    {
        PlayMusic(_victoryMusic);
    }

    private void PlayMusic(AudioClip musicClip)
    {
        if (!_musicEnabled || musicClip == null || _musicSource == null) return;

        if (_musicSource.clip != musicClip)
        {
            _musicSource.clip = musicClip;
            _musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (_musicSource != null)
        {
            _musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (_musicSource != null && _musicEnabled)
        {
            _musicSource.UnPause();
        }
    }
    #endregion

    #region Sound Effects
    public void PlayHealthPickupSound()
    {
        PlaySFX(_healthPickupSound);
    }

    public void PlayFireballSound()
    {
        PlaySFX(_fireballSound);
    }

    public void PlayExplosionSound()
    {
        PlaySFX(_explosionSound);
    }

    public void PlayPlayerHitSound()
    {
        PlaySFX(_playerHitSound);
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(_buttonClickSound);
    }

    private void PlaySFX(AudioClip sfxClip)
    {
        if (!_sfxEnabled || sfxClip == null || _sfxSource == null) return;

        _sfxSource.PlayOneShot(sfxClip);
    }
    #endregion

    #region Audio Settings
    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void ToggleMusic(bool enabled)
    {
        _musicEnabled = enabled;
        UpdateVolumes();
    }

    public void ToggleSFX(bool enabled)
    {
        _sfxEnabled = enabled;
        UpdateVolumes();
    }

    public float GetMusicVolume() => _musicVolume;
    public float GetSFXVolume() => _sfxVolume;
    public bool IsMusicEnabled() => _musicEnabled;
    public bool IsSFXEnabled() => _sfxEnabled;
    #endregion

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
                PlayMainMenuMusic();
                break;
            case "GamePlay":
                PlayGameplayMusic();
                break;
            case "VictoryScene":
                PlayVictoryMusic();
                break;
            // Add other scenes if needed
            default:
                StopMusic();
                break;
        }
    }
} 