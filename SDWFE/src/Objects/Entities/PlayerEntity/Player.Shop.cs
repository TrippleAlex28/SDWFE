using SDWFE.UI.Shop;

public partial class Player
{
    /// <summary>
    /// The player's shop UI.
    /// </summary>
    private UIShop? _shopUI;

    /// <summary>
    /// Gets the player's shop UI.
    /// </summary>
    public UIShop? ShopUI => _shopUI;

    /// <summary>
    /// Constructs the player's shop UI.
    /// </summary>
    private void ConstructShopUI()
    {
        _shopUI = new UIShop();
    }
}