using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip _loginMusic;
    [SerializeField] private AudioClip _homeMusic;
    [SerializeField] private AudioClip _lobbyMusic;
    [SerializeField] private AudioClip _pvpMusic;
    [SerializeField] private AudioClip _pveMusic;
    [SerializeField] private AudioClip _bossMusic;
    [SerializeField] private AudioClip _defeatMusic;
    [SerializeField] private AudioClip _settingsMusic;
    [SerializeField] private AudioClip _victoryMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip _fireballSound;
    [SerializeField] private AudioClip _explosionSound;
    [SerializeField] private AudioClip _playerHitSound;
    [SerializeField] private AudioClip _buttonClickSound;
    [SerializeField] private AudioClip _buttonCloseSound;
    [SerializeField] private AudioClip _buttonPopupSound;
    [SerializeField] private AudioClip _notEnoughSound;
    [SerializeField] private AudioClip _buySuccessSound;

    [SerializeField] private AudioClip _gameModeSelectSound;
    [SerializeField] private AudioClip _hunterSelect;
    [SerializeField] private AudioClip _buttonChangeSound;

    [Header("Sound Effects InGame")]
    [SerializeField] private AudioClip _healthRecoverSound;
    [SerializeField] private AudioClip _getXpItemSound;
    [SerializeField] private AudioClip _skillRunSound;

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
        
        LoadSettings();
        InitializeAudioSources();
    }

    private void Start()
    {
        PlayLoginMusic();
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
    public void PlayHomeMusic()
    {
        PlayMusic(_homeMusic);
    }

    public void PlayPVEMusic()
    {
        PlayMusic(_pveMusic);
    }

    public void PlayVictoryMusic()
    {
        PlayMusic(_victoryMusic);
    }

    public void PlayLoginMusic()
    {
        PlayMusic(_loginMusic);
    }

    public void PlayLobbyMusic()
    {
        PlayMusic(_lobbyMusic);
    }

    public void PlayPVPMusic()
    {
        PlayMusic(_pvpMusic);
    }

    public void PlayDefeatMusic()
    {
        PlayMusic(_defeatMusic);
    }

    public void PlaySettingsMusic()
    {
        PlayMusic(_settingsMusic);
    }

    private void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null || _musicSource == null) return;

        if (_musicSource.clip != musicClip)
        {
            _musicSource.clip = musicClip;
            _musicSource.loop = true;
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

    public void PlayFireballSound()
    {
        PlaySFX(_fireballSound);
    }

    public void PlayFireballSoundAtPosition(Vector3 position)
    {
        PlaySFXAtPosition(_fireballSound, position);
    }

    public void PlayExplosionSound()
    {
        PlaySFX(_explosionSound);
    }

    public void PlayExplosionSoundAtPosition(Vector3 position)
    {
        PlaySFXAtPosition(_explosionSound, position);
    }

    public void PlayPlayerHitSound()
    {
        PlaySFX(_playerHitSound);
    }

    public void PlayButtonClickSound()
    {
        PlaySFX(_buttonClickSound);
    }

    public void PlayButtonCloseSound()
    {
        PlaySFX(_buttonCloseSound);
    }

    public void PlayButtonPopupSound()
    {
        PlaySFX(_buttonPopupSound);
    }

    public void PlayNotEnoughSound()
    {
        PlaySFX(_notEnoughSound);
    }

    public void PlayBuySuccessSound()
    {
        PlaySFX(_buySuccessSound);
    }

    private void PlaySFX(AudioClip sfxClip)
    {
        if (!_sfxEnabled || sfxClip == null || _sfxSource == null) return;

        _sfxSource.PlayOneShot(sfxClip);
    }

    private void PlaySFXAtPosition(AudioClip sfxClip, Vector3 position)
    {
        if (!_sfxEnabled || sfxClip == null) return;
        GameObject tempAudio = new GameObject("TempAudio");
        tempAudio.transform.position = position;
        
        AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
        audioSource.clip = sfxClip;
        audioSource.volume = _sfxVolume; 
        audioSource.spatialBlend = 1.0f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.maxDistance = 50f;
        audioSource.minDistance = 1f;
        
        audioSource.Play();
        Destroy(tempAudio, sfxClip.length);
    }

    #region Audio Settings
    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveSettings();
    }

    public void SetSFXVolume(float volume)
    {
        _sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
        SaveSettings();
    }

    public void ToggleMusic(bool enabled)
    {
        _musicEnabled = enabled;
        UpdateVolumes();
        SaveSettings();
    }

    public void ToggleSFX(bool enabled)
    {
        _sfxEnabled = enabled;
        UpdateVolumes();
        SaveSettings();
    }

    public float GetMusicVolume() => _musicVolume;
    public float GetSFXVolume() => _sfxVolume;
    public bool IsMusicEnabled() => _musicEnabled;
    public bool IsSFXEnabled() => _sfxEnabled;

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", _musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", _sfxVolume);
        PlayerPrefs.SetInt("MusicEnabled", _musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("SFXEnabled", _sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        _sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        _musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        _sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;
    }
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

    public void PlayMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            // Login & Authentication Scenes
            case GameConstants.SCENE_LOGIN:
                PlayLoginMusic();
                break;
            
            // Main Menu & UI Scenes
            case GameConstants.SCENE_HOME:
                PlayHomeMusic();
                break;
            
            // Lobby & Matchmaking
            case GameConstants.SCENE_LOBBY:
                PlayLobbyMusic();
                break;
            
            case GameConstants.SCENE_PVE_FOREST:
                PlayPVEMusic();
                break;
            
            case GameConstants.SCENE_PVP_FOREST:
                PlayPVPMusic();
                break;
            default:
                Debug.LogWarning($"No music found for scene: {sceneName}");
                StopMusic();
                break;
        }
    }

    public void PlayHunterSelectSound()
    {
        PlaySFX(_hunterSelect);
    }

    public void PlayGameModeSelectSound()
    {
        PlaySFX(_gameModeSelectSound);
    }

    public void PlayButtonChangeSound()
    {
        PlaySFX(_buttonChangeSound);
    }

    public void PlayHealthRecoverSound()
    {
        PlaySFX(_healthRecoverSound);
    }

    public void PlayGetXpItemSound()
    {
        PlaySFX(_getXpItemSound);
    }

    public void PlaySkillRunSound()
    {
        PlaySFX(_skillRunSound);
    }
} 