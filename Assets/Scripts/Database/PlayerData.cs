using UnityEngine;
using System;

[Serializable]
public class PlayerData
{
    // Basic info
    public string userId;
    public string email;
    public string displayName;
    public DateTime createdAt;
    public DateTime lastLoginTime;
    
    // Game stats
    public int totalKills;
    public int totalDeaths;
    public int gamesPlayed;
    public int gamesWon;
    public float totalPlayTime;
    public int totalDamageDealt;
    public int totalDamageReceived;
    
    // Settings
    public string selectedCharacter;
    public string selectedSkill;
    public bool soundEnabled;
    public bool musicEnabled;
    public float masterVolume;
    public float sfxVolume;
    public float musicVolume;
    
    // Achievements and unlocks
    public bool[] unlockedAchievements;
    public int achievementPoints;
    public string[] unlockedCharacters;
    public string[] unlockedSkills;
    public string[] unlockedCosmetics;

    // PlayerUpdate
    public float damage;
    public float ammor;
    public int level;
    public float xp;
    public float gold;
    public float ruby;
    public float cash;

    public int food;
    public int health;

    public string playerPrefabName;

    
    
    // Constructor for new player
    public PlayerData(string email, string displayName, string userId)
    {
        this.email = email;
        this.displayName = displayName;
        this.userId = userId;
        this.createdAt = DateTime.Now;
        this.lastLoginTime = DateTime.Now;
        
        this.totalKills = 0;
        this.totalDeaths = 0;
        this.gamesPlayed = 0;
        this.gamesWon = 0;
        this.totalPlayTime = 0f;
        this.totalDamageDealt = 0;
        this.totalDamageReceived = 0;
        
        this.selectedCharacter = "Default";
        this.selectedSkill = "FireBall";
        this.soundEnabled = true;
        this.musicEnabled = true;
        this.masterVolume = 1.0f;
        this.sfxVolume = 1.0f;
        this.musicVolume = 0.8f;
        
        this.unlockedAchievements = new bool[10];
        this.achievementPoints = 0;
        this.unlockedCharacters = new string[] { "Default" };
        this.unlockedSkills = new string[] { "FireBall" };
        this.unlockedCosmetics = new string[0];

        this.damage = 200;
        this.ammor = 0;
        this.level = 1;
        this.xp = 0;
        this.gold = 100;
        this.ruby = 100;
        this.cash = 10000;
        this.health = 1000;
        this.food = 5;
        this.playerPrefabName = "Player_Mage";
    }
    
    private string GenerateRandomPlayerName()
    {
        string[] prefixes = { "Wizard", "Mage", "Sorcerer", "Warlock", "Enchanter", "Spellcaster", "Mystic", "Arcane" };
        string[] suffixes = { "Fire", "Ice", "Lightning", "Shadow", "Storm", "Flame", "Frost", "Thunder", "Dark", "Light" };
        
        string prefix = prefixes[UnityEngine.Random.Range(0, prefixes.Length)];
        string suffix = suffixes[UnityEngine.Random.Range(0, suffixes.Length)];
        int number = UnityEngine.Random.Range(1, 1000);
        
        return $"{prefix}{suffix}{number}";
    }
    
    public void UpdateStats(int kills, int deaths, bool won, float playTime, int damageDealt, int damageReceived)
    {
        totalKills += kills;
        totalDeaths += deaths;
        gamesPlayed++;
        if (won) gamesWon++;
        totalPlayTime += playTime;
        totalDamageDealt += damageDealt;
        totalDamageReceived += damageReceived;
        lastLoginTime = DateTime.Now;
    }
    
    public void UnlockCharacter(string characterName)
    {
        if (!Array.Exists(unlockedCharacters, x => x == characterName))
        {
            Array.Resize(ref unlockedCharacters, unlockedCharacters.Length + 1);
            unlockedCharacters[unlockedCharacters.Length - 1] = characterName;
        }
    }
    
    public void UnlockSkill(string skillName)
    {
        if (!Array.Exists(unlockedSkills, x => x == skillName))
        {
            Array.Resize(ref unlockedSkills, unlockedSkills.Length + 1);
            unlockedSkills[unlockedSkills.Length - 1] = skillName;
        }
    }
    
    public void UnlockCosmetic(string cosmeticName)
    {
        if (!Array.Exists(unlockedCosmetics, x => x == cosmeticName))
        {
            Array.Resize(ref unlockedCosmetics, unlockedCosmetics.Length + 1);
            unlockedCosmetics[unlockedCosmetics.Length - 1] = cosmeticName;
        }
    }
}
