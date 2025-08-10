using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

public class FirebaseDataManager : MonoBehaviour
{
    public static FirebaseDataManager Instance { get; private set; }

    [Header("Firebase Configuration")]
    [SerializeField] private string databaseUrl = "https://wizardbattle-b3901-default-rtdb.firebaseio.com/";

    private DatabaseReference databaseReference;
    private FirebaseAuth auth;
    private bool isInitialized = false;

    // Events
    public event Action<PlayerData> OnPlayerDataLoaded;
    public event Action<bool> OnPlayerDataSaved;
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
        Debug.Log("FirebaseDataManager Start() called");
        // Delay initialization to ensure Firebase is ready
        Invoke(nameof(InitializeFirebase), 0.1f);
    }

    private void InitializeFirebase()
    {
        Debug.Log("Starting Firebase initialization...");
        
        try
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogError($"Firebase dependency check failed: {task.Exception.Message}");
                    OnError?.Invoke($"Firebase dependency check failed: {task.Exception.Message}");
                    return;
                }
                
                if (task.Result == DependencyStatus.Available)
                {
                    try
                    {
                        Debug.Log("Firebase dependencies available, initializing...");
                        
                        // Initialize Auth
                        auth = FirebaseAuth.DefaultInstance;
                        if (auth == null)
                        {
                            Debug.LogError("Failed to get Firebase Auth instance");
                            OnError?.Invoke("Failed to get Firebase Auth instance");
                            return;
                        }
                        
                        // Initialize Database
                        FirebaseDatabase dbInstance = null;
                        if (!string.IsNullOrEmpty(databaseUrl))
                        {
                            try
                            {
                                dbInstance = FirebaseDatabase.GetInstance(databaseUrl);
                            }
                            catch (Exception getDbEx)
                            {
                                Debug.LogWarning($"Failed to get FirebaseDatabase with custom URL, falling back to DefaultInstance. Reason: {getDbEx.Message}");
                            }
                        }
                        databaseReference = (dbInstance ?? FirebaseDatabase.DefaultInstance).RootReference;
                        if (databaseReference == null)
                        {
                            Debug.LogError("Failed to get Firebase Database reference");
                            OnError?.Invoke("Failed to get Firebase Database reference");
                            return;
                        }
                        
                        isInitialized = true;
                        Debug.Log("Firebase Data Manager initialized successfully");
                        
                        // Set up auth state changed listener
                        auth.StateChanged += AuthStateChanged;
                        AuthStateChanged(this, null);
                        
                        // Notify that initialization is complete
                        Debug.Log("Firebase Data Manager is ready to use");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error during Firebase initialization: {e.Message}");
                        Debug.LogError($"Stack trace: {e.StackTrace}");
                        OnError?.Invoke($"Firebase initialization error: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogError($"Firebase dependencies not available. Status: {task.Result}");
                    OnError?.Invoke($"Firebase dependencies not available. Status: {task.Result}");
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting Firebase initialization: {e.Message}");
            OnError?.Invoke($"Error starting Firebase initialization: {e.Message}");
        }
    }
    
    private void AuthStateChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log($"User authenticated: {auth.CurrentUser.Email}");
        }
        else
        {
            Debug.Log("User signed out");
        }
    }

    public async Task<bool> SavePlayerData(PlayerData playerData)
    {
        if (!isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return false;
        }

        try
        {
            // Use the userId from the playerData if auth.CurrentUser is null
            string userId = auth.CurrentUser?.UserId ?? playerData.userId;
            
            if (string.IsNullOrEmpty(userId))
            {
                OnError?.Invoke("No user ID available for saving data");
                return false;
            }
            
            Debug.Log($"Attempting to save player data for user: {userId}");
            
            string jsonData = JsonUtility.ToJson(playerData);
            
            // Add timeout to prevent hanging
            var timeoutTask = Task.Delay(10000); // 10 second timeout
            var saveTask = databaseReference.Child("players").Child(userId).SetRawJsonValueAsync(jsonData);
            
            var completedTask = await Task.WhenAny(saveTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Debug.LogError("Timeout while saving player data");
                OnError?.Invoke("Timeout while saving player data");
                OnPlayerDataSaved?.Invoke(false);
                return false;
            }
            
            if (saveTask.IsFaulted)
            {
                HandleTaskFault("saving player data", saveTask.Exception);
                OnPlayerDataSaved?.Invoke(false);
                return false;
            }
            
            await saveTask;
            
            Debug.Log($"Player data saved successfully for user: {userId}");
            OnPlayerDataSaved?.Invoke(true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving player data: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            OnError?.Invoke($"Error saving player data: {e.Message}");
            OnPlayerDataSaved?.Invoke(false);
            return false;
        }
    }

    public async Task<PlayerData> LoadPlayerData()
    {
        if (!isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return null;
        }

        try
        {
            string userId = auth.CurrentUser?.UserId;
            
            if (string.IsNullOrEmpty(userId))
            {
                Debug.Log("No authenticated user, cannot load player data");
                return null;
            }
            
            Debug.Log($"Attempting to load player data for user: {userId}");
            
            // Add timeout to prevent hanging
            var timeoutTask = Task.Delay(10000); // 10 second timeout
            var loadTask = databaseReference.Child("players").Child(userId).GetValueAsync();
            
            var completedTask = await Task.WhenAny(loadTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Debug.LogError("Timeout while loading player data");
                OnError?.Invoke("Timeout while loading player data");
                return null;
            }
            
            if (loadTask.IsFaulted)
            {
                HandleTaskFault("loading player data", loadTask.Exception);
                return null;
            }
            
            var snapshot = await loadTask;
            
            if (snapshot.Exists)
            {
                string jsonData = snapshot.GetRawJsonValue();
                if (string.IsNullOrEmpty(jsonData))
                {
                    Debug.LogError($"Player data snapshot exists but JSON is empty for user: {userId}");
                    OnError?.Invoke("Player data exists but JSON is empty");
                    return null;
                }
                PlayerData playerData = null;
                try
                {
                    playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                }
                catch (Exception jsonEx)
                {
                    Debug.LogError($"Failed to parse player data JSON: {jsonEx.Message}");
                    Debug.LogError($"JSON content: {jsonData}");
                    OnError?.Invoke($"Failed to parse player data JSON: {jsonEx.Message}");
                    return null;
                }
                
                Debug.Log($"Player data loaded successfully for user: {userId}");
                OnPlayerDataLoaded?.Invoke(playerData);
                return playerData;
            }
            else
            {
                Debug.Log($"No existing player data found for user: {userId}");
                return null; // Return null so calling code can create new data
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading player data: {e}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            OnError?.Invoke($"Error loading player data: {e.Message}");
            return null;
        }
    }

    public async Task<bool> UpdatePlayerStats(int kills, int deaths, bool won, float playTime, int damageDealt, int damageReceived)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        currentData.UpdateStats(kills, deaths, won, playTime, damageDealt, damageReceived);
        return await SavePlayerData(currentData);
    }

    public async Task<bool> UpdatePlayerSettings(string selectedCharacter, string selectedSkill, bool soundEnabled, bool musicEnabled, float masterVolume, float sfxVolume, float musicVolume)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        currentData.selectedCharacter = selectedCharacter;
        currentData.selectedSkill = selectedSkill;
        currentData.soundEnabled = soundEnabled;
        currentData.musicEnabled = musicEnabled;
        currentData.masterVolume = masterVolume;
        currentData.sfxVolume = sfxVolume;
        currentData.musicVolume = musicVolume;

        return await SavePlayerData(currentData);
    }

    public async Task<bool> UnlockAchievement(int achievementIndex)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        if (achievementIndex >= 0 && achievementIndex < currentData.unlockedAchievements.Length)
        {
            currentData.unlockedAchievements[achievementIndex] = true;
            currentData.achievementPoints += 10; // Add points for unlocking achievement
            return await SavePlayerData(currentData);
        }

        return false;
    }

    public async Task<bool> UnlockCharacter(string characterName)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        currentData.UnlockCharacter(characterName);
        return await SavePlayerData(currentData);
    }

    public async Task<bool> UnlockSkill(string skillName)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        currentData.UnlockSkill(skillName);
        return await SavePlayerData(currentData);
    }

    public async Task<bool> UnlockCosmetic(string cosmeticName)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        currentData.UnlockCosmetic(cosmeticName);
        return await SavePlayerData(currentData);
    }

    public async Task<List<PlayerData>> GetLeaderboard(int limit = 10)
    {
        if (!isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return new List<PlayerData>();
        }

        try
        {
            var snapshot = await databaseReference.Child("players").OrderByChild("totalKills").LimitToLast(limit).GetValueAsync();
            List<PlayerData> leaderboard = new List<PlayerData>();

            foreach (var childSnapshot in snapshot.Children)
            {
                string jsonData = childSnapshot.GetRawJsonValue();
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(jsonData);
                leaderboard.Add(playerData);
            }

            // Sort by total kills descending
            leaderboard.Sort((a, b) => b.totalKills.CompareTo(a.totalKills));
            return leaderboard;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading leaderboard: {e.Message}");
            OnError?.Invoke($"Error loading leaderboard: {e.Message}");
            return new List<PlayerData>();
        }
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }
    
    public bool IsUserAuthenticated()
    {
        return auth?.CurrentUser != null;
    }

    public string GetCurrentUserId()
    {
        return auth?.CurrentUser?.UserId;
    }

    public string GetCurrentUserEmail()
    {
        return auth?.CurrentUser?.Email;
    }

    public string GetCurrentUserDisplayName()
    {
        return auth?.CurrentUser?.DisplayName;
    }

    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
    }

    private void HandleTaskFault(string context, AggregateException aggregate)
    {
        try
        {
            if (aggregate == null)
            {
                Debug.LogError($"Task faulted during {context}, but no exception provided.");
                return;
            }

            var flattened = aggregate.Flatten();
            foreach (var inner in flattened.InnerExceptions)
            {
                Debug.LogError($"Task fault during {context}: {inner.GetType().Name}: {inner.Message}\n{inner.StackTrace}");
                if (inner is FirebaseException firebaseEx)
                {
                    Debug.LogError($"Firebase error (code {(int)firebaseEx.ErrorCode}): {firebaseEx.Message}");
                }
            }

            var first = flattened.InnerExceptions.Count > 0 ? flattened.InnerExceptions[0] : null;
            if (first != null)
            {
                OnError?.Invoke($"Task fault during {context}: {first.Message}");
            }
            else
            {
                OnError?.Invoke($"Task fault during {context}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while handling task fault for {context}: {ex.Message}");
        }
    }
}
