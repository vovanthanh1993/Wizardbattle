using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public float pvpXpReward = 200;
    public float pvpGoldReward = 100;
    public float pvpRubyReward = 2;

    public float pveXpReward = 200;
    public float pveGoldReward = 100;
    public float pveRubyReward = 2;

    public List<MissionReward> missionRewards;

    public List<ShopData> shopData;

    public GameData()
    {
        missionRewards = new List<MissionReward>(); // Khởi tạo List rỗng
    }
}
public enum MissionType {
    Daily,
    Weekly,
    Monthly,

    LevelUp,

    Event,

    Challenge,
}

[Serializable]
public class MissionReward {
    public MissionType missionType;
    public int missionId;
    public string missionName;
    public int xpReward;
    public int goldReward;
    public int rubyReward;
    public int foodReward;
    public string missionDescription;
    public int levelRequirement;
}
