using Engine.Input;
using SDWFE.UI.Shop;

#nullable enable

namespace SDWFE.Objects.Entities.PlayerEntity;
public partial class Player
{
    /// <summary>
    /// The player's shop UI.
    /// </summary>
    private UIShop? _shopUI;
    
    /// <summary>
    /// Whether the shop is currently open
    /// </summary>
    private bool _isShopOpen = false;

    /// <summary>
    /// Gets the player's shop UI.
    /// </summary>
    public UIShop? ShopUI => _shopUI;

    /// <summary>
    /// Constructs the player's shop UI.
    /// </summary>
    private void ConstructShopUI()
    {
        _shopUI = new UIShop(this);
    }
    
    /// <summary>
    /// Toggles the shop UI visibility
    /// </summary>
    public void ToggleShop()
    {
        if (_shopUI == null) return;
        
        _isShopOpen = !_isShopOpen;
        if (_isShopOpen)
        {
            _shopUI.Show();
            OnWindowOpen();
        }
        else
        {
            OnWindowClosed();
            _shopUI.Hide();
        }
    }

    public void CloseShop()
    {
        if (_shopUI == null) return;
        
        if (_isShopOpen)
        {
            _isShopOpen = false;
            OnWindowClosed();
            _shopUI.Hide();
        }
    }
    private void OnWindowClosed()
    {
        if (HotbarUI != null) HotbarUI.IsVisible = true;
        if (WeaponsUI != null) WeaponsUI.IsVisible = true; 
        if (StatsUI != null) StatsUI.IsVisible = true; 
    }

    private void OnWindowOpen()
    {
        if (HotbarUI != null) HotbarUI.IsVisible = false; 
        if (WeaponsUI != null) WeaponsUI.IsVisible = false; 
        if (StatsUI != null) StatsUI.IsVisible = false;
    }
    /// <summary>
    /// Updates shop input (called from UpdateSelf)
    /// </summary>
    private void UpdateShop()
    {
        var input = InputManager.Instance;
        
        // Toggle shop with Tab key (Inventory action)
        if (input.IsActionPressed(InputSetup.ACTION_INVENTORY))
        {
            ToggleShop();
        }
    }
}