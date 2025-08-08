using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

[Serializable]
public class UserData
{
    public string userId;
    public string email;
    public string displayName;
    public int level;
    public int experience;
    public int coins;
    public int wins;
    public int losses;
    public int kills;
    public int deaths;
    public DateTime lastLogin;
    public List<string> unlockedSpells;
    
    public UserData()
    {
        unlockedSpells = new List<string>();
    }
}

public class FirebaseAuthSystem : MonoBehaviour
{
    public static FirebaseAuthSystem Instance { get; private set; }
    
    [Header("Firebase Configuration")]
    [SerializeField] private string _projectId = "wizardbattle-b3901";
    [SerializeField] private string _apiKey = "1:381819246771:android:9826aaba8a056bb83e78c6";
    
    [Header("How to Update Firebase Config")]
    [TextArea(8, 12)]
    [SerializeField] private string _configInstructions = "TO UPDATE FIREBASE CONFIG:\n\n" +
        "1. Go to Firebase Console\n" +
        "2. Project Settings > General\n" +
        "3. Copy Project ID and API Key\n" +
        "4. Update _projectId and _apiKey above\n" +
        "5. Authentication > Settings for Auth Domain\n" +
        "6. Realtime Database for Database URL";
    
    private bool _isInitialized = false;
    private UserData _currentUser;
    
    public bool IsLoggedIn => _currentUser != null;
    public UserData CurrentUser => _currentUser;
    
    public event Action<bool> OnLoginResult;
    public event Action<bool> OnRegisterResult;
    public event Action<UserData> OnUserDataLoaded;
    public event Action<string> OnError;
    
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
            // TODO: Initialize Firebase SDK
            // FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            //     var dependencyStatus = task.Result;
            //     if (dependencyStatus == DependencyStatus.Available)
            //     {
            //         _isInitialized = true;
            //         Debug.Log("Firebase initialized successfully");
            //     }
            // });
            
            _isInitialized = true;
            Debug.Log("Firebase initialized successfully");
            
            // Check if user is already logged in
            await CheckExistingLogin();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize Firebase: {e.Message}");
            OnError?.Invoke($"Firebase initialization failed: {e.Message}");
        }
    }
    
    private async Task CheckExistingLogin()
    {
        try
        {
            string savedUserId = PlayerPrefs.GetString("UserId", "");
            if (!string.IsNullOrEmpty(savedUserId))
            {
                await LoadUserData(savedUserId);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to check existing login: {e.Message}");
        }
    }
    
    public async Task<bool> RegisterUser(string email, string password, string displayName)
    {
        if (!_isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return false;
        }
        
        try
        {
            // TODO: Implement actual Firebase Auth registration
            // var userCredential = await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            // var user = userCredential.User;
            
            // For demo purposes, create a mock user
            string userId = "user_" + Guid.NewGuid().ToString();
            
            // Create user data
            _currentUser = new UserData
            {
                userId = userId,
                email = email,
                displayName = displayName,
                level = 1,
                experience = 0,
                coins = 100,
                wins = 0,
                losses = 0,
                kills = 0,
                deaths = 0,
                lastLogin = DateTime.Now,
                unlockedSpells = new List<string> { "Fireball" }
            };
            
            // Save to Firebase Database
            await SaveUserData(_currentUser);
            
            // Save to local storage
            PlayerPrefs.SetString("UserId", _currentUser.userId);
            PlayerPrefs.Save();
            
            OnRegisterResult?.Invoke(true);
            OnUserDataLoaded?.Invoke(_currentUser);
            
            Debug.Log($"User registered successfully: {displayName}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Registration failed: {e.Message}");
            OnError?.Invoke($"Registration failed: {e.Message}");
            OnRegisterResult?.Invoke(false);
            return false;
        }
    }
    
    public async Task<bool> LoginUser(string email, string password)
    {
        if (!_isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return false;
        }
        
        try
        {
            // TODO: Implement actual Firebase Auth login
            // var userCredential = await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
            // var user = userCredential.User;
            
            // For demo purposes, create a mock login
            string userId = "user_" + email.GetHashCode();
            bool success = await LoadUserData(userId);
            
            if (success)
            {
                // Update last login
                _currentUser.lastLogin = DateTime.Now;
                await SaveUserData(_currentUser);
                
                // Save to local storage
                PlayerPrefs.SetString("UserId", _currentUser.userId);
                PlayerPrefs.Save();
                
                OnLoginResult?.Invoke(true);
                OnUserDataLoaded?.Invoke(_currentUser);
                
                Debug.Log($"User logged in successfully: {_currentUser.displayName}");
                return true;
            }
            else
            {
                OnLoginResult?.Invoke(false);
                OnError?.Invoke("Invalid credentials");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Login failed: {e.Message}");
            OnError?.Invoke($"Login failed: {e.Message}");
            OnLoginResult?.Invoke(false);
            return false;
        }
    }
    
    public async Task<bool> LoadUserData(string userId)
    {
        try
        {
            // TODO: Implement actual Firebase Database loading
            // var snapshot = await FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(userId).GetValueAsync();
            // if (snapshot.Exists)
            // {
            //     _currentUser = JsonUtility.FromJson<UserData>(snapshot.GetRawJsonValue());
            // }
            
            // For demo purposes, create mock data
            _currentUser = new UserData
            {
                userId = userId,
                email = "player@example.com",
                displayName = "Player_" + userId.Substring(0, 5),
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
            
            OnUserDataLoaded?.Invoke(_currentUser);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load user data: {e.Message}");
            OnError?.Invoke($"Failed to load user data: {e.Message}");
            return false;
        }
    }
    
    public async Task<bool> SaveUserData(UserData userData)
    {
        try
        {
            // TODO: Implement actual Firebase Database saving
            // string json = JsonUtility.ToJson(userData);
            // await FirebaseDatabase.DefaultInstance.RootReference.Child("users").Child(userData.userId).SetRawJsonValueAsync(json);
            
            Debug.Log($"User data saved: {userData.displayName}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save user data: {e.Message}");
            OnError?.Invoke($"Failed to save user data: {e.Message}");
            return false;
        }
    }
    
    public async Task UpdateUserStatistics(int kills, int deaths, bool won)
    {
        if (_currentUser == null) return;
        
        _currentUser.kills += kills;
        _currentUser.deaths += deaths;
        
        if (won)
        {
            _currentUser.wins++;
            _currentUser.experience += 100;
            _currentUser.coins += 50;
        }
        else
        {
            _currentUser.losses++;
            _currentUser.experience += 25;
        }
        
        // Level up check
        int newLevel = (_currentUser.experience / 1000) + 1;
        if (newLevel > _currentUser.level)
        {
            _currentUser.level = newLevel;
            _currentUser.coins += 200; // Level up bonus
            Debug.Log($"Level up! New level: {newLevel}");
        }
        
        await SaveUserData(_currentUser);
    }
    
    public void Logout()
    {
        _currentUser = null;
        PlayerPrefs.DeleteKey("UserId");
        PlayerPrefs.Save();
        Debug.Log("User logged out");
    }
    
    // Helper method to validate email format
    public bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    // Helper method to validate password strength
    public bool IsValidPassword(string password)
    {
        return password.Length >= 6;
    }
} 