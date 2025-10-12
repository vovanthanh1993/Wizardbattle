using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FirebaseImageHelper : MonoBehaviour
{
    public static FirebaseImageHelper Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private int maxCacheSize = 50;
    [SerializeField] private bool enableDebugLogs = true;
    
    // Cache system
    private Dictionary<string, Sprite> imageCache = new Dictionary<string, Sprite>();
    private Dictionary<string, bool> loadingUrls = new Dictionary<string, bool>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load image from Firebase URL to Image component
    /// </summary>
    /// <param name="imageComponent">Image component to display the image</param>
    /// <param name="imageUrl">Firebase Storage URL</param>
    /// <param name="defaultSprite">Default sprite to show when error occurs (optional)</param>
    public void LoadImageToComponent(Image imageComponent, string imageUrl, Sprite defaultSprite = null)
    {
        UIManager.Instance.ShowLoadingPanel(true);
        if (imageComponent == null)
        {
            LogError("Image component is null!");
            return;
        }
        
        if (string.IsNullOrEmpty(imageUrl))
        {
            LogWarning("Image URL is null or empty!");
            if (defaultSprite != null)
                imageComponent.sprite = defaultSprite;
            return;
        }
        
        // Check cache first
        if (imageCache.ContainsKey(imageUrl))
        {
            LogDebug($"Loading image from cache: {imageUrl}");
            imageComponent.sprite = imageCache[imageUrl];
            UIManager.Instance.ShowLoadingPanel(false);
            return;
        }
        
        // Check if already loading
        if (loadingUrls.ContainsKey(imageUrl) && loadingUrls[imageUrl])
        {
            LogDebug($"Image already loading: {imageUrl}");
            StartCoroutine(WaitForImageLoad(imageComponent, imageUrl, defaultSprite));
            UIManager.Instance.ShowLoadingPanel(false);
            return;
        }
        
        // Start loading
        loadingUrls[imageUrl] = true;
        StartCoroutine(LoadImageCoroutine(imageComponent, imageUrl, defaultSprite));
        
    }
    
    private IEnumerator LoadSpriteCoroutine(string imageUrl)
    {
        LogDebug($"Loading sprite from URL: {imageUrl}");
        
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            www.timeout = 10; // 10 seconds timeout
            yield return www.SendWebRequest();
            
            // Mark as not loading
            loadingUrls[imageUrl] = false;
            
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                
                if (texture != null)
                {
                    // Create Sprite from Texture2D
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                    
                    // Cache the sprite
                    CacheSprite(imageUrl, sprite);
                    LogDebug("Sprite loaded and cached successfully!");
                }
                else
                {
                    LogError("Failed to create texture!");
                }
            }
            else
            {
                LogError($"Failed to load sprite. Error: {www.error}");
            }
        }
    }
    
    private IEnumerator LoadImageCoroutine(Image imageComponent, string imageUrl, Sprite defaultSprite)
    {
        LogDebug($"Loading image from URL: {imageUrl}");
        
        using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            www.timeout = 10; // 10 seconds timeout
            yield return www.SendWebRequest();
            
            // Mark as not loading
            loadingUrls[imageUrl] = false;
            
            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(www);
                
                if (texture != null && imageComponent != null)
                {
                    // Create Sprite from Texture2D
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f
                    );
                    
                    // Cache the sprite
                    CacheSprite(imageUrl, sprite);
                    
                    // Assign image to Image component
                    imageComponent.sprite = sprite;
                    LogDebug("Image loaded successfully!");
                    UIManager.Instance.ShowLoadingPanel(false);
                }
                else
                {
                    LogError("Failed to create texture!");
                    ShowDefaultImage(imageComponent, defaultSprite);
                }
            }
            else
            {
                LogError($"Failed to load image. Error: {www.error}");
                ShowDefaultImage(imageComponent, defaultSprite);
            }
        }
    }
    
    private IEnumerator WaitForImageLoad(Image imageComponent, string imageUrl, Sprite defaultSprite)
    {
        // Wait until image is loaded or timeout
        float timeout = 10f;
        float elapsed = 0f;
        
        while (loadingUrls.ContainsKey(imageUrl) && loadingUrls[imageUrl] && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        // Check if image is now in cache
        if (imageCache.ContainsKey(imageUrl))
        {
            imageComponent.sprite = imageCache[imageUrl];
        }
        else
        {
            ShowDefaultImage(imageComponent, defaultSprite);
        }
    }
    
    private void ShowDefaultImage(Image imageComponent, Sprite defaultSprite)
    {
        if (imageComponent != null && defaultSprite != null)
        {
            imageComponent.sprite = defaultSprite;
        }
    }
    
    private void CacheSprite(string imageUrl, Sprite sprite)
    {
        // Check cache size limit
        if (imageCache.Count >= maxCacheSize)
        {
            // Remove oldest entry (simple FIFO)
            var firstKey = "";
            foreach (var key in imageCache.Keys)
            {
                firstKey = key;
                break;
            }
            
            if (!string.IsNullOrEmpty(firstKey))
            {
                imageCache.Remove(firstKey);
                LogDebug($"Removed oldest cached image: {firstKey}");
            }
        }
        
        imageCache[imageUrl] = sprite;
        LogDebug($"Cached sprite: {imageUrl}");
    }
    
    private void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"FirebaseImageHelper: {message}");
        }
    }
    
    private void LogWarning(string message)
    {
        Debug.LogWarning($"FirebaseImageHelper: {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"FirebaseImageHelper: {message}");
    }
    
    /// <summary>
    /// Overload function - just pass Image component and URL
    /// </summary>
    /// <param name="imageComponent">Image component to display the image</param>
    /// <param name="imageUrl">Firebase Storage URL</param>
    public void SetImage(Image imageComponent, string imageUrl)
    {
        LoadImageToComponent(imageComponent, imageUrl, null);
    }
    
    /// <summary>
    /// Check if Singleton has been initialized
    /// </summary>
    public static bool IsInitialized => Instance != null;
    
    /// <summary>
    /// Stop all running coroutines
    /// </summary>
    public void StopAllImageLoading()
    {
        StopAllCoroutines();
        loadingUrls.Clear();
        LogDebug("Stopped all image loading coroutines");
    }
    
    /// <summary>
    /// Clear image cache
    /// </summary>
    public void ClearCache()
    {
        imageCache.Clear();
        loadingUrls.Clear();
        LogDebug("Cache cleared");
    }
    
    /// <summary>
    /// Clear specific image from cache
    /// </summary>
    public void ClearCache(string imageUrl)
    {
        if (imageCache.ContainsKey(imageUrl))
        {
            imageCache.Remove(imageUrl);
            LogDebug($"Removed from cache: {imageUrl}");
        }
        
        if (loadingUrls.ContainsKey(imageUrl))
        {
            loadingUrls.Remove(imageUrl);
        }
    }
    
    /// <summary>
    /// Get cache info
    /// </summary>
    public int GetCacheSize()
    {
        return imageCache.Count;
    }
    
    /// <summary>
    /// Check if image is cached
    /// </summary>
    public bool IsImageCached(string imageUrl)
    {
        return imageCache.ContainsKey(imageUrl);
    }
    
    /// <summary>
    /// Static helper method to get instance and call SetImage
    /// </summary>
    /// <param name="imageComponent">Image component to display the image</param>
    /// <param name="imageUrl">Firebase Storage URL</param>
    public static void LoadImage(Image imageComponent, string imageUrl)
    {
        if (Instance == null)
        {
            Debug.LogError("FirebaseImageHelper: Instance is null! Make sure FirebaseImageHelper is in the scene.");
            return;
        }
        
        Instance.SetImage(imageComponent, imageUrl);
    } 
}