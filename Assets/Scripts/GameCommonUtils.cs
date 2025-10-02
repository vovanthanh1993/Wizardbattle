using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CoroutineRunner : MonoBehaviour
{
    
}
public static class GameCommonUtils 
{
    private static MonoBehaviour _coroutineRunner;
    
    private static MonoBehaviour CoroutineRunner
    {
        get
        {
            if (_coroutineRunner == null)
            {
                GameObject go = new GameObject("CoroutineRunner");
                _coroutineRunner = go.AddComponent<CoroutineRunner>();
                Object.DontDestroyOnLoad(go);
            }
            return _coroutineRunner;
        }
    }

    public static void LoadScene(string sceneName)
    {
        CoroutineRunner.StartCoroutine(LoadSceneAsync(sceneName));
    }

    private static IEnumerator LoadSceneAsync(string sceneName)
    {
        UIManager.Instance.ShowLoadingPanel(true);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            yield return null;
        }

        UIManager.Instance.ShowLoadingPanel(false);
    }

    // Get game time as string (mm:ss)
    public static string GetGameTimeString(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public static Sprite GetItemTypeSprite(ItemType itemType, MaterialTier materialTier)
    {
        string spritePath = $"ItemIcons/{itemType}/{itemType}_{materialTier}";
        Sprite sprite = Resources.Load<Sprite>(spritePath);
        return sprite;
    }

    public static Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common:
                return new Color(0.8f, 0.8f, 0.8f, 1f); // Màu xám nhạt
            case Rarity.Rare:
                return new Color(0.2f, 0.6f, 1f, 1f); // Màu xanh dương sáng
            case Rarity.Epic:
                return new Color(0.8f, 0.2f, 1f, 1f); // Màu tím sáng
            case Rarity.Legendary:
                return new Color(1f, 0.8f, 0.2f, 1f); // Màu vàng cam
            default:
                return Color.white;
        }
    }

    public static GameObject GetItemPrefab(ItemType itemType, string materialTier)
    {
        string prefabPath = $"ItemPrefabs/{itemType}/{itemType}_{materialTier}";
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        return prefab;
    }

    
    // XP System Configuration
    private const int BASE_XP = 100;
    private const float XP_MULTIPLIER = 1.5f;
    
    /// <summary>
    /// Calculate level from total XP using exponential growth formula (float version)
    /// </summary>
    /// <param name="totalXP">Total XP accumulated</param>
    /// <returns>Current level</returns>
    public static int CalculateLevelFromXP(float totalXP)
    {
        if (totalXP <= 0) return 1;
        
        int level = Mathf.FloorToInt(Mathf.Sqrt(totalXP / 50f));
        return level;
    }
    
    /// <summary>
    /// Calculate XP needed to reach a specific level
    /// Level 1: 0 XP
    /// Level 2: 200 XP (100-299 range)
    /// Level 3: 450 XP (300-599 range)  
    /// Level 4: 800 XP (600-999 range)
    /// Level 5: 1250 XP (1000-1499 range)
    /// Formula: maxXP = level^2 * 50
    /// </summary>
    /// <param name="level">Target level</param>
    /// <returns>XP needed to reach that level</returns>
    public static long CalculateXPForLevel(int level)
    {
        if (level <= 1) return 0;
        return level * level * 50;
    }
    
    /// <summary>
    /// Calculate XP needed to go from one level to another
    /// </summary>
    /// <param name="fromLevel">Starting level</param>
    /// <param name="toLevel">Target level</param>
    /// <returns>XP needed to go from fromLevel to toLevel</returns>
    public static long CalculateXPBetweenLevels(int fromLevel, int toLevel)
    {
        if (fromLevel >= toLevel) return 0;
        
        long fromXP = CalculateXPForLevel(fromLevel);
        long toXP = CalculateXPForLevel(toLevel);
        
        return toXP - fromXP;
    }
    
    /// <summary>
    /// Calculate XP needed to reach next level from current level
    /// </summary>
    /// <param name="currentLevel">Current level</param>
    /// <returns>XP needed to reach next level</returns>
    public static long CalculateXPToNextLevel(int currentLevel)
    {
        return CalculateXPBetweenLevels(currentLevel, currentLevel + 1);
    }
    
    /// <summary>
    /// Calculate current level XP progress (XP within current level)
    /// </summary>
    /// <param name="totalXP">Total XP accumulated</param>
    /// <param name="currentLevel">Current level</param>
    /// <returns>XP progress within current level</returns>
    public static float CalculateCurrentLevelXP(float totalXP, int currentLevel)
    {
        if (currentLevel <= 1) return totalXP;
        
        long xpForCurrentLevel = CalculateXPForLevel(currentLevel);
        return totalXP - xpForCurrentLevel;
    }
    
    /// <summary>
    /// Calculate XP progress percentage for current level
    /// </summary>
    /// <param name="totalXP">Total XP accumulated</param>
    /// <param name="currentLevel">Current level</param>
    /// <returns>Progress percentage (0-1)</returns>
    public static float CalculateXPProgressPercentage(float totalXP, int currentLevel)
    {
        float currentLevelXP = CalculateCurrentLevelXP(totalXP, currentLevel);
        float xpToNextLevel = CalculateXPToNextLevel(currentLevel);
        
        if (xpToNextLevel <= 0) return 1f;
        
        return Mathf.Clamp01(currentLevelXP / xpToNextLevel);
    }
}
