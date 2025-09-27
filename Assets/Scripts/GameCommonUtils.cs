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
}
