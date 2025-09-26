using System;

[Serializable]
public class EquipmentData
{
    public string equippedHelmet = "";
    public string equippedArmor = "";
    public string equippedGloves = "";
    public string equippedBoots = "";
    public string equippedRing = "";
    public string equippedWeapon = "";
    public string equippedRune = "";
    public string equippedBook = "";
    
    public int totalDamageBonus = 0;
    public int totalSpeedBonus = 0;
    public int totalHealthBonus = 0;
    
    public EquipmentData()
    {
        // Initialize with empty values
    }
}
