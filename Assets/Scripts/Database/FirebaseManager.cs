using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public string userId;
    public string username;
    public string email;
    public int level;
    public int experience;
    public int coins;
    public int wins;
    public int losses;
    public int kills;
    public int deaths;
    public DateTime lastLogin;
    public List<string> unlockedSpells;
    public Dictionary<string, int> statistics;
    
    public PlayerData()
    {
        unlockedSpells = new List<string>();
        statistics = new Dictionary<string, int>();
    }
}

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    
    [Header("Firebase Settings")]
    [SerializeField] private string firebaseProjectId;
    [SerializeField] private string firebaseApiKey;
    
    private bool _isInitialized = false;
    private PlayerData _currentPlayerData;
    
    public bool IsLoggedIn => _currentPlayerData != null;
    public PlayerData CurrentPlayerData => _currentPlayerData;
    
    public event Action<bool> OnLoginResult;
    public event Action<bool> OnRegisterResult;
    public event Action<PlayerData> OnPlayerDataLoaded;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        InitializeFirebase();
    }
    
    private async void InitializeFirebase()
    {
        try
        {
            // Initialize Firebase SDK
            // Note: You'll need to add Firebase SDK to your project
            // Download from: https://firebase.google.com/docs/unity/setup
            
            _isInitialized = true;
            Debug.Log("Firebase initialized successfully");
            
            // Check if user is already logged in
            await CheckExistingLogin();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Firebase: {e.Message}");
        }
    }
    
    private async Task CheckExistingLogin()
    {
        // Check if user has saved credentials
        string savedUserId = PlayerPrefs.GetString("UserId", "");
        if (!string.IsNullOrEmpty(savedUserId))
        {
            await LoadPlayerData(savedUserId);
        }
    }
    
    public async Task<bool> RegisterUser(string email, string password, string username)
    {
        if (!_isInitialized)
        {
            Debug.LogError("Firebase not initialized");
            return false;
        }
        
        try
        {
            // Create user account with Firebase Auth
            // var userCredential = await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            
            // Create player data
            _currentPlayerData = new PlayerData
            {
                userId = "user_" + System.Guid.NewGuid().ToString(),
                username = username,
                email = email,
                level = 1,
                experience = 0,
                coins = 100, // Starting coins
                wins = 0,
                losses = 0,
                kills = 0,
                deaths = 0,
                lastLogin = DateTime.Now,
                unlockedSpells = new List<string> { "Fireball" } // Default spell
            };
            
            // Save to Firebase Database
            await SavePlayerData(_currentPlayerData);
            
            // Save to local storage
            PlayerPrefs.SetString("UserId", _currentPlayerData.userId);
            PlayerPrefs.Save();
            
            OnRegisterResult?.Invoke(true);
            OnPlayerDataLoaded?.Invoke(_currentPlayerData);
            
            Debug.Log($"User registered successfully: {username}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Registration failed: {e.Message}");
            OnRegisterResult?.Invoke(false);
            return false;
        }
    }
    
    public async Task<bool> LoginUser(string email, string password)
    {
        if (!_isInitialized)
        {
            Debug.LogError("Firebase not initialized");
            return false;
        }
        
        try
        {
            // Sign in with Firebase Auth
            // var userCredential = await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
            
            // Load player data
            string userId = "user_" + email.GetHashCode(); // Simplified for demo
            bool success = await LoadPlayerData(userId);
            
            if (success)
            {
                // Update last login
                _currentPlayerData.lastLogin = DateTime.Now;
                await SavePlayerData(_currentPlayerData);
                
                // Save to local storage
                PlayerPrefs.SetString("UserId", _currentPlayerData.userId);
                PlayerPrefs.Save();
                
                OnLoginResult?.Invoke(true);
                OnPlayerDataLoaded?.Invoke(_currentPlayerData);
                
                Debug.Log($"User logged in successfully: {_currentPlayerData.username}");
                return true;
            }
            else
            {
                OnLoginResult?.Invoke(false);
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Login failed: {e.Message}");
            OnLoginResult?.Invoke(false);
            return false;
        }
    }
    
    public async Task<bool> LoadPlayerData(string userId)
    {
        try
        {
            // Load from Firebase Database
            // var snapshot = await FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(userId).GetValueAsync();
            
            // For demo purposes, create mock data
            _currentPlayerData = new PlayerData
            {
                userId = userId,
                username = "Player_" + userId.Substring(0, 5),
                email = "player@example.com",
                level = UnityEngine.Random.Range(1, 10),
                experience = UnityEngine.Random.Range(0, 1000),
                coins = UnityEngine.Random.Range(100, 1000),
                wins = UnityEngine.Random.Range(0, 50),
                losses = UnityEngine.Random.Range(0, 30),
                kills = UnityEngine.Random.Range(0, 200),
                deaths = UnityEngine.Random.Range(0, 150),
                lastLogin = DateTime.Now,
                unlockedSpells = new List<string> { "Fireball", "IceSpell", "Lightning" }
            };
            
            OnPlayerDataLoaded?.Invoke(_currentPlayerData);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load player data: {e.Message}");
            return false;
        }
    }
    
    public async Task<bool> SavePlayerData(PlayerData playerData)
    {
        try
        {
            // Save to Firebase Database
            // string json = JsonUtility.ToJson(playerData);
            // await FirebaseDatabase.DefaultInstance.RootReference.Child("players").Child(playerData.userId).SetRawJsonValueAsync(json);
            
            Debug.Log($"Player data saved: {playerData.username}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save player data: {e.Message}");
            return false;
        }
    }
    
    public async Task UpdatePlayerStatistics(int kills, int deaths, bool won)
    {
        if (_currentPlayerData == null) return;
        
        _currentPlayerData.kills += kills;
        _currentPlayerData.deaths += deaths;
        
        if (won)
        {
            _currentPlayerData.wins++;
            _currentPlayerData.experience += 100;
            _currentPlayerData.coins += 50;
        }
        else
        {
            _currentPlayerData.losses++;
            _currentPlayerData.experience += 25;
        }
        
        // Level up check
        int newLevel = (_currentPlayerData.experience / 1000) + 1;
        if (newLevel > _currentPlayerData.level)
        {
            _currentPlayerData.level = newLevel;
            _currentPlayerData.coins += 200; // Level up bonus
            Debug.Log($"Level up! New level: {newLevel}");
        }
        
        await SavePlayerData(_currentPlayerData);
    }
    
    public void Logout()
    {
        _currentPlayerData = null;
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.Save();
        Debug.Log("User logged out");
    }
} 