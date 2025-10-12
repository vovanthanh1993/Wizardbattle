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
    [SerializeField] private string databaseUrl = "https://winzardbattle-default-rtdb.firebaseio.com/";

    private DatabaseReference databaseReference;
    private FirebaseAuth auth;
    private bool isInitialized = false;
    private PlayerData currentPlayerData;
    private GameData currentGameData;

    // Events
    public event Action<PlayerData> OnPlayerDataLoaded;
    public event Action<GameData> OnGameDataLoaded;
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
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
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
                        
                        // Auto create game data
                        CreateInitialGameData();
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
            currentPlayerData = playerData;
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
                currentPlayerData = playerData;
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

    public async Task<GameData> LoadGameData()
    {
        if (!isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return null;
        }

        try
        {
            Debug.Log("Attempting to load game data");
            
            // Add timeout to prevent hanging
            var timeoutTask = Task.Delay(10000); // 10 second timeout
            var loadTask = databaseReference.Child("gamedata").GetValueAsync();
            
            var completedTask = await Task.WhenAny(loadTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Debug.LogError("Timeout while loading game data");
                OnError?.Invoke("Timeout while loading game data");
                return null;
            }
            
            if (loadTask.IsFaulted)
            {
                HandleTaskFault("loading game data", loadTask.Exception);
                return null;
            }
            
            var snapshot = await loadTask;
            
            if (snapshot.Exists)
            {
                string jsonData = snapshot.GetRawJsonValue();
                if (string.IsNullOrEmpty(jsonData))
                {
                    Debug.LogError("Game data snapshot exists but JSON is empty");
                    OnError?.Invoke("Game data exists but JSON is empty");
                    return null;
                }
                
                GameData gameData = null;
                try
                {
                    gameData = JsonUtility.FromJson<GameData>(jsonData);
                }
                catch (Exception jsonEx)
                {
                    Debug.LogError($"Failed to parse game data JSON: {jsonEx.Message}");
                    Debug.LogError($"JSON content: {jsonData}");
                    OnError?.Invoke($"Failed to parse game data JSON: {jsonEx.Message}");
                    return null;
                }
                
                Debug.Log("Game data loaded successfully");
                currentGameData = gameData;
                OnGameDataLoaded?.Invoke(gameData);
                return gameData;
            }
            else
            {
                Debug.Log("No existing game data found");
                return null; // Return null so calling code can create new data
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading game data: {e}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            OnError?.Invoke($"Error loading game data: {e.Message}");
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

    public async Task<bool> UpdatePlayerAttributes(int damage, int speed, int level, float xp, float gold, float ruby, int food)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        currentData.damage = damage;
        currentData.speed = speed;          
        currentData.level = level;
        currentData.xp = xp;
        currentData.gold = gold;
        currentData.ruby = ruby;
        currentData.food = food;

        return await SavePlayerData(currentData);
    }

    public async Task<bool> UpdatePlayerAttributes(PlayerData playerData)
    {
        PlayerData currentData = await LoadPlayerData();
        if (currentData == null)
        {
            return false;
        }

        currentData.damage = playerData.damage;
        currentData.speed = playerData.speed;
        currentData.level = playerData.level;
        currentData.xp = playerData.xp;
        currentData.gold = playerData.gold;
        currentData.ruby = playerData.ruby;

        return await SavePlayerData(currentData);
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
        if (currentPlayerData != null)
        {
            return currentPlayerData.displayName;
        }
        return "Unknown Player";
    }

    public PlayerData GetCurrentPlayerData()
    {
        return currentPlayerData;
    }

    public GameData GetCurrentGameData()
    {
        return currentGameData;
    }

    public int GetCurrentUserDamage()
    {
        return currentPlayerData.damage;
    }

    public int GetCurrentUserSpeed()
    {
        return currentPlayerData.speed;
    }

    public int GetCurrentUserLevel()
    {
        return currentPlayerData.level;
    }

    public float GetCurrentUserXp()
    {
        return currentPlayerData.xp;
    }

    public float GetCurrentUserMaxXp()
    {
        int currentLevel = GetCurrentUserLevel();
        return GameCommonUtils.CalculateXPForLevel(currentLevel + 1);
    }

    public float GetCurrentUserGold()
    {
        return currentPlayerData.gold;
    }

    public float GetCurrentUserRuby()
    {
        return currentPlayerData.ruby;
    }

    public float GetCurrentUserHealth()
    {
        return currentPlayerData.health;
    }

    public int GetCurrentUserFood()
    {
        return currentPlayerData.food;
    }

    public float GetCurrentUserCash()
    {
        return currentPlayerData.cash;
    }

    public async Task<bool> BuyGold(int ruby, int gold)
    {
        currentPlayerData.ruby -= ruby;
        currentPlayerData.gold += gold;
        return await SavePlayerData(currentPlayerData);
    }

    public async Task<bool> BuyFood(int ruby, int food)
    {
        currentPlayerData.ruby -= ruby;
        currentPlayerData.food += food;
        return await SavePlayerData(currentPlayerData);
    }

    public async Task<bool> BuyRuby(float cash, int ruby)
    {
        currentPlayerData.cash -= cash;
        currentPlayerData.ruby += ruby;
        return await SavePlayerData(currentPlayerData);
    }

    public async Task<bool> ResetToDefault()
    {
        currentPlayerData.baseDamage = 200;
        currentPlayerData.baseSpeed = 500;
        currentPlayerData.level = 1;
        currentPlayerData.xp = 0;
        currentPlayerData.gold = 100;
        currentPlayerData.ruby = 100;
        currentPlayerData.cash = 10000;
        currentPlayerData.baseHealth = 1000;
        currentPlayerData.food = 0;
        return await SavePlayerData(currentPlayerData);
    }

    public async Task<bool> Upgrade(int gold, UpgradeType upgradeType, int upgradeAmount)
    {
        currentPlayerData.gold -= gold;
        switch (upgradeType)
        {
            case UpgradeType.Health:
                currentPlayerData.health += upgradeAmount;
                break;
            case UpgradeType.Damage:
                currentPlayerData.damage += upgradeAmount;
                break;
            case UpgradeType.Speed:
                currentPlayerData.speed += upgradeAmount;
                break;
        }
        return await SavePlayerData(currentPlayerData);
    }

    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
        }
    }

    /// <summary>
    /// Save inventory data to Firebase (updates PlayerData)
    /// </summary>
    public async Task<bool> SaveInventory(InventoryData inventoryData)
    {
        if (!isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return false;
        }

        try
        {
            // Update current player data with new inventory
            if (currentPlayerData != null)
            {
                currentPlayerData.inventoryData = inventoryData;
                // Save the entire PlayerData (which includes inventory)
                return await SavePlayerData(currentPlayerData);
            }
            else
            {
                OnError?.Invoke("No current player data available");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving inventory data: {e.Message}");
            OnError?.Invoke($"Error saving inventory data: {e.Message}");
            return false;
        }
    }


    /// <summary>
    /// Get current inventory data from memory
    /// </summary>
    public InventoryData GetCurrentInventoryData()
    {
        return currentPlayerData?.inventoryData ?? new InventoryData();
    }

    /// <summary>
    /// Buy random item with random stats and save to database
    /// </summary>
    public async Task<bool> BuyRandomItem(int goldCost, InventoryItem randomItem)
    {
        try {
            currentPlayerData.gold -= goldCost;
            if (currentPlayerData.inventoryData == null)
            {
                currentPlayerData.inventoryData = new InventoryData();
            }
            currentPlayerData.inventoryData.AddRandomItem(randomItem, 1);
            bool success = await SavePlayerData(currentPlayerData);
            return success;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error buying random item: {e.Message}");
            OnError?.Invoke($"Error buying random item: {e.Message}");
            return false;
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

    public async Task UpdatePlayerAttributesAfterGame(float xpReward, float goldReward, float rubyReward)
    {
        try
        {  
            float newXP = currentPlayerData.xp;
            float newGold = currentPlayerData.gold;
            float newRuby = currentPlayerData.ruby;

            newXP += xpReward;
            newGold += goldReward;
            newRuby += rubyReward;
            
            // Check and update level
            int newLevel = GameCommonUtils.CalculateLevelFromXP(newXP);
            
            // Update currentData with new values before saving
            currentPlayerData.level = newLevel;
            currentPlayerData.xp = newXP;
            currentPlayerData.gold = newGold;
            currentPlayerData.ruby = newRuby;
            
            // Update PlayerData
            await SavePlayerData(currentPlayerData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in UpdatePlayerAttributesAfterGame: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    public async Task<bool> ClaimMissionReward(MissionReward missionReward)
    {
        currentPlayerData.gold += missionReward.goldReward;
        currentPlayerData.ruby += missionReward.rubyReward;
        currentPlayerData.food += missionReward.foodReward;
        currentPlayerData.CompleteMission(missionReward.missionId);
        return await SavePlayerData(currentPlayerData);
    }

    public async Task<bool> SaveGameData(GameData gameData)
    {
        if (!isInitialized)
        {
            OnError?.Invoke("Firebase not initialized");
            return false;
        }

        try
        {
            Debug.Log("Attempting to save game data");
            
            string jsonData = JsonUtility.ToJson(gameData);
            
            // Add timeout to prevent hanging
            var timeoutTask = Task.Delay(10000); // 10 second timeout
            var saveTask = databaseReference.Child("gamedata").SetRawJsonValueAsync(jsonData);
            
            var completedTask = await Task.WhenAny(saveTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Debug.LogError("Timeout while saving game data");
                OnError?.Invoke("Timeout while saving game data");
                return false;
            }
            
            if (saveTask.IsFaulted)
            {
                HandleTaskFault("saving game data", saveTask.Exception);
                return false;
            }
            
            await saveTask;
            
            Debug.Log("Game data saved successfully");
            currentGameData = gameData;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving game data: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
            OnError?.Invoke($"Error saving game data: {e.Message}");
            return false;
        }
    }


    /// <summary>
    /// Create initial game data
    /// </summary>
    public async Task<bool> CreateInitialGameData()
    {
        try
        {
            GameData initialGameData = new GameData();
            
            // Tạo missions mẫu
            initialGameData.missionRewards = new List<MissionReward>
            {
                new MissionReward
                {
                    missionType = MissionType.LevelUp,
                    missionId = 1,
                    missionName = "Novice Reward",
                    missionDescription = "Reach level 2 to unlock rewards",
                    levelRequirement = 2,
                    xpReward = 0,
                    goldReward = 50,
                    rubyReward = 1,
                    foodReward = 1,
                },
                new MissionReward
                {
                    missionType = MissionType.LevelUp,
                    missionId = 2,
                    missionName = "Apprentice Reward",
                    missionDescription = "Reach level 5 to unlock rewards",
                    levelRequirement = 5,
                    xpReward = 0,
                    goldReward = 100,
                    rubyReward = 2,
                    foodReward = 1,
                },
                new MissionReward
                {
                    missionType = MissionType.LevelUp,
                    missionId = 3,
                    missionName = "Adept Reward",
                    missionDescription = "Reach level 8 for ruby rewards",
                    levelRequirement = 8,
                    xpReward = 0,
                    goldReward = 75,
                    rubyReward = 1,
                    foodReward = 1,
                },
                new MissionReward   
                {
                    missionType = MissionType.LevelUp,
                    missionId = 4,
                    missionName = "Expert Reward",
                    missionDescription = "Reach level 10 for food rewards",
                    levelRequirement = 10,
                    xpReward = 0,
                    goldReward = 150,
                    rubyReward = 3,
                    foodReward = 1,
                },
                new MissionReward {
                    missionType = MissionType.LevelUp,
                    missionId = 5,
                    missionName = "Master Reward",
                    missionDescription = "Reach level 12 for food rewards",
                    levelRequirement = 12,
                    xpReward = 0,
                    goldReward = 200,
                    rubyReward = 4,
                    foodReward = 2,
                },
                new MissionReward {
                    missionType = MissionType.LevelUp,
                    missionId = 6,
                    missionName = "Grandmaster Reward",
                    missionDescription = "Reach level 15 for food rewards",
                    levelRequirement = 15,
                    xpReward = 0,
                    goldReward = 250,
                    rubyReward = 5,
                    foodReward = 3,
                }, 
                new MissionReward {
                    missionType = MissionType.LevelUp,
                    missionId = 7,
                    missionName = "Legendary Reward",
                    missionDescription = "Reach level 20 for food rewards",
                    levelRequirement = 20,
                    xpReward = 0,
                    goldReward = 300,
                    rubyReward = 6,
                    foodReward = 4,
                }  
            };

            // Create shop data
            initialGameData.shopData = new List<ShopData>
            {
                new ShopData
                {
                    buyType = ShopType.Ruby,
                    paidType = ShopType.Gold,
                    shopName = "Ruby Pack - Small",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Fruby_1.png?alt=media&token=4b99568c-0f6f-4a76-8532-f0ca19eea370",
                    buyAmount = 50,
                    paidAmount = 0.99f,
                    width = 109,
                    height = 169
                },
                new ShopData
                {
                    buyType = ShopType.Ruby,
                    paidType = ShopType.Gold,
                    shopName = "Ruby Pack - Medium",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Fruby_2.png?alt=media&token=73e0d8f0-9398-4fca-840c-4eec609da699",
                    buyAmount = 200,
                    paidAmount = 1.99f,
                    width = 147,
                    height = 175
                },
                new ShopData
                {
                    buyType = ShopType.Ruby,
                    paidType = ShopType.Gold,
                    shopName = "Ruby Pack - Large",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Fruby_3.png?alt=media&token=e0c73f10-7a73-4b22-8e59-da7cb0f8a0b4",
                    buyAmount = 500,
                    paidAmount = 2.5f,
                    width = 159,
                    height = 175
                },
                new ShopData
                {
                    buyType = ShopType.Gold,
                    paidType = ShopType.Ruby,
                    shopName = "Gold Pack - Small",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Fgold_1.png?alt=media&token=eb2dff1a-8aac-4a67-821a-eb244498707f",
                    buyAmount = 200,
                    paidAmount = 50f,
                    width = 155,
                    height = 120
                },
                new ShopData
                {
                    buyType = ShopType.Gold,
                    paidType = ShopType.Ruby,
                    shopName = "Gold Pack - Medium",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Fgold_2.png?alt=media&token=35344a59-aaf2-4640-9376-ff7a416c1a8c",
                    buyAmount = 500,
                    paidAmount = 120f,
                    width = 155,
                    height = 120
                },
                new ShopData
                {
                    buyType = ShopType.Gold,
                    paidType = ShopType.Ruby,
                    shopName = "Gold Pack - Large",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Fgold_3.png?alt=media&token=061e6ace-57f0-4456-885d-72a784ee31a2",
                    buyAmount = 1000,
                    paidAmount = 200f,
                    width = 155,
                    height = 120
                },
                new ShopData
                {
                    buyType = ShopType.Food,
                    paidType = ShopType.Ruby,
                    shopName = "Food Pack - Small",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Ffood_1.png?alt=media&token=6749df43-ffa8-4c39-b89d-8df33936089e",
                    buyAmount = 10,
                    paidAmount = 50f,
                    width = 160,
                    height = 130
                },
                new ShopData
                {
                    buyType = ShopType.Food,
                    paidType = ShopType.Ruby,
                    shopName = "Food Pack - Medium",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Ffood_2.png?alt=media&token=83a9e12f-1169-48dc-a1ea-c9edee4439c3",
                    buyAmount = 30,
                    paidAmount = 140f,
                    width = 160,
                    height = 130
                },
                new ShopData
                {
                    buyType = ShopType.Food,
                    paidType = ShopType.Ruby,
                    shopName = "Food Pack - Large",
                    imageURL = "https://firebasestorage.googleapis.com/v0/b/winzardbattle.firebasestorage.app/o/ShopPackage%2Ffood_3.png?alt=media&token=0f47c368-a8b2-464b-8401-51988bc89b67",
                    buyAmount = 60,
                    paidAmount = 250f,
                    width = 160,
                    height = 130
                }
            };

            Debug.Log("Creating initial game data...");
            Debug.Log($"JSON Data: {JsonUtility.ToJson(initialGameData, true)}");
            
            return await SaveGameData(initialGameData);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error creating initial game data: {e.Message}");
            return false;
        }
    }
}
