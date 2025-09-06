using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayPanel : MonoBehaviour
{
    [SerializeField] private Image _xpBarImage;
    [SerializeField] private TMP_Text _levelText;

    [Header("Level Calculation")]
    [SerializeField] private int _baseXP = 100;
    [SerializeField] private float _xpMultiplier = 1.5f;

    [Header("Skill UI")]
    [SerializeField] private GameObject _skillUI1;
    [SerializeField] private GameObject _skillUI2;
    [SerializeField] private GameObject _skillUI3;
    public bool IsEnableSkill1 { get; private set; }
    public bool IsEnableSkill2 { get; private set; }
    public bool IsEnableSkill3 { get; private set; }
    public void UpdateXpBar(long xp, long xpToNextLevel)
    {
        if (_xpBarImage != null)
        {
            float progress = (float)xp / xpToNextLevel;
            _xpBarImage.fillAmount = Mathf.Clamp01(progress);
        }
    }

    public void UpdateLevel(int level)
    {
        if (_levelText != null)
        {
            _levelText.text = $"Level {level}";
        }
    }
    
    public void UpdateLevelUI(long xp)
    {
        int level = CalculateLevelFromXP(xp);
        IsEnableSkill1 = level >= 5;
        IsEnableSkill2 = level >= 8;
        IsEnableSkill3 = level >= 10;
        _skillUI1.SetActive(!IsEnableSkill1);
        _skillUI2.SetActive(!IsEnableSkill2);
        _skillUI3.SetActive(!IsEnableSkill3);
        long currentLevelXP = CalculateCurrentLevelXP(xp, level);
        long xpToNextLevel = CalculateXPToNextLevel(level);
        
        UpdateLevel(level);
        UpdateXpBar(currentLevelXP, xpToNextLevel);
    }
    
    private int CalculateLevelFromXP(long xp)
    {
        if (xp <= 0) return 1;
        
        int level = 1;
        long totalXPNeeded = 0;
        
        while (totalXPNeeded <= xp)
        {
            level++;
            totalXPNeeded += CalculateXPForLevel(level);
        }
        return level - 1;
    }
    
    private long CalculateXPForLevel(int level)
    {
        if (level <= 1) return 0;
        return (long)(_baseXP * Mathf.Pow(_xpMultiplier, level - 2));
    }
    
    private long CalculateCurrentLevelXP(long totalXP, int currentLevel)
    {
        if (currentLevel <= 1) return totalXP;
        
        long xpForCurrentLevel = 0;
        for (int i = 2; i <= currentLevel; i++)
        {
            xpForCurrentLevel += CalculateXPForLevel(i);
        }
        
        return totalXP - xpForCurrentLevel;
    }
    
    private long CalculateXPToNextLevel(int currentLevel)
    {
        return CalculateXPForLevel(currentLevel + 1);
    }
}
