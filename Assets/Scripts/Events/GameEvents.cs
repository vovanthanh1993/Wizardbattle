using System;

public static class GameEvents
{
    // Event khi mua hàng thành công
    public static event Action<ShopType> OnShopPurchaseSuccess;
    
    // Event khi cần cập nhật số liệu shop
    public static event Action OnUpdateShopNumbers;
    
    // Event khi cần cập nhật UI
    public static event Action OnUpdateUI;
    
    /// <summary>
    /// Trigger event khi mua hàng thành công
    /// </summary>
    /// <param name="shopType">Loại hàng đã mua</param>
    public static void TriggerShopPurchaseSuccess(ShopType shopType)
    {
        OnShopPurchaseSuccess?.Invoke(shopType);
    }
    
    /// <summary>
    /// Trigger event cập nhật số liệu shop
    /// </summary>
    public static void TriggerUpdateShopNumbers()
    {
        OnUpdateShopNumbers?.Invoke();
    }
    
    /// <summary>
    /// Trigger event cập nhật UI
    /// </summary>
    public static void TriggerUpdateUI()
    {
        OnUpdateUI?.Invoke();
    }
}
