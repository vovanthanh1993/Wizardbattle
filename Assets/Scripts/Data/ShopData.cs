using UnityEngine;
using System;

public enum ShopType {
    Ruby,
    Gold,
    Food
}

[Serializable]
public class ShopData
{
    public ShopType buyType;
    public ShopType paidType;
    public string shopName;
    public string imageURL;
    public int buyAmount;
    public float paidAmount;

    public float width;
    public float height;
}
