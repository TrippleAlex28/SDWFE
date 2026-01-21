using Engine.UI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Engine;

using System;
using System.Collections.Generic;
using Engine.UI.Elements;
using SDWFE.Objects.Entities.PlayerEntity;
using System.IO;
using SDWFE.Objects.Inventory.Item;

#nullable enable

namespace SDWFE.UI.Shop;

/// <summary>
/// ShopOverlay displays a shop window with 5 purchasable items and an exit button.
/// </summary>
public sealed class UIShop: UIElement
{

    public event Action<string, int>? OnItemPurchased;

    private readonly UIRoot _rootElement;
    private UIHBoxContainer? _shopHeader;
    private Player? _owner;
    
    // UI elements
    private UIVisual? _windowBackground;
    private UIControl? _exitButton;
    private UIVisual? _exitButtonVisual;
    private UIVBoxContainer? _itemsContainer;
    
    // Shop items
    private ShopItem[] _shopItems = new ShopItem[5];
    
    // Coins and purchase tracking
    public int _coins = 550;
    
    private SpriteFont? _font24;
    private SpriteFont? _font;
    private Texture2D? _shopTexture;
    private Texture2D? _buyButtonTexture;

    private static readonly string[] _shopItemList =
    [
        ItemSetup.BANDAGE,
        ItemSetup.ADRENALINE,
        ItemSetup.FREEZE,
        ItemSetup.RAGE,
        ItemSetup.SLAM
    ];
    
    public UIShop(Player player) : base()
    {
        _owner = player;
        _coins = _owner.Stats.Coins;
        _owner.Stats.OnStatsChanged += (StatType type, bool decreased) => OnCoinsChanged(_owner.Stats.Coins);
        
        _rootElement = new UIRoot();
        Rectangle screenRect = new Rectangle(0, 0, 480, 270);
        _rootElement.SetRootRect(screenRect);
        this.AddChild(_rootElement);

        // Load assets
        _font = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 12);
        _font24 = Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 24);
        _shopTexture = ExtendedGame.AssetManager.LoadTexture("inventorySheet", "UI/");
        _buyButtonTexture = ExtendedGame.AssetManager.LoadTexture("UI_AddButton", "UI/");

        BuildShopWindow();
        
        Hide(); // Shop hidden by default
    }

    private void BuildShopWindow()
    {
        // Create the main shop window container (square-ish centered window)
        UIVBoxContainer shopWindow = new UIVBoxContainer();
        shopWindow.DesiredSize = new Vector2(300, 252);
        shopWindow.AlignmentPoint = Alignment.MiddleCenter;
        shopWindow.Padding = new Vector4(15, 15, 15, 15);
        
        // Window background using solid color
        _windowBackground = UIVisual.FromStretchableTexture(
            _shopTexture!,
            new Vector4(12, 12, 12, 12),
            new Rectangle(0, 0, 32, 32)
        );
        _windowBackground.DesiredSize = new Vector2(300, 252);
        _windowBackground.AlignmentPoint = Alignment.MiddleCenter;
        
        // Add background to window
        _windowBackground.AddChild(shopWindow);

        _shopHeader = new UIHBoxContainer();
        _shopHeader.DesiredSize = new Vector2(300,32);
        
        // Create title
        UIVisual titleText = UIVisual.FromText("SHOP", _font24!, Color.Black);
        titleText.DesiredSize = new Vector2(268, 16);
        titleText.SetDesiredSize();
        _shopHeader.AddChild(titleText);

        // Create exit button
        BuildExitButton();
        shopWindow.AddChild(_shopHeader);

        // Create container for shop items
        _itemsContainer = new UIVBoxContainer();
        _itemsContainer.DesiredSize = new Vector2(300, 200);
        _itemsContainer.Spacing = 5f;

        // Build 5 shop items from abilities
        for (int i = 0; i < _shopItemList.Length; i++)
        {
            var itemData = ItemDatabase.Instance.GetItemData(_shopItemList[i]);
            int price = itemData.Price;
            
            _shopItems[i] = CreateShopItem(itemData.Name, price, i);
            
            // Set ability icon
            _shopItems[i].Icon!.source = ExtendedGame.AssetManager.LoadTexture(itemData.IconPath, "Items/");
            _shopItems[i].Icon!.SourceRect = null;
            
            _itemsContainer.AddChild(_shopItems[i].Container!);
        }


        //setItems();
        shopWindow.AddChild(_itemsContainer);

        _rootElement.AddChild(_windowBackground);
        
        // Add exit button on top layer
    }
    // private void setItems()
    // {
    //     var inventory = ItemFactory.ShopInventory;
    //     for (int i = 0; i < inventory.Count && i < _shopItems.Length; i++)
    //     {
    //         var entry = inventory[i];
    //         var ability = ItemFactory.CreateAbility(entry.Name);
    //         if (ability != null)
    //             SetItemDetails(i, entry.Name, entry.Price, ability.IconTexture, ability.IconTextureRectangle);
    //         else
    //             SetItemDetails(i, entry.Name, entry.Price);
    //     }
    // }

    private void BuildExitButton()
    {
        // Create exit button control - positioned absolutely on the root
        _exitButton = new UIControl();
        _exitButton.DesiredSize = new Vector2(16, 16);
        _exitButton.AlignmentPoint = Alignment.TopRight;

        // Exit button background
        _exitButtonVisual = UIVisual.FromTexture(
            _buyButtonTexture!,
            new Rectangle(96, 0, 16, 16)
        );
        _exitButtonVisual.Tint = Color.White;
        _exitButtonVisual.SetDesiredSize();
        _exitButtonVisual.AlignmentPoint = Alignment.MiddleRight;


        // Add visuals to button
        _exitButton.AddChild(_exitButtonVisual);
        // _exitButton.AddChild(_exitButtonText);

        // Wire up button events
        _exitButton.HoverEntered += OnExitButtonHover;
        _exitButton.HoverExited += OnExitButtonUnhover;
        _exitButton.Released += OnExitButtonClicked;

        _shopHeader!.AddChild(_exitButton);
    }

    private ShopItem CreateShopItem(string itemName, int price, int index)
    {
        ShopItem item = new ShopItem();
        
        // Item container
        UIHBoxContainer itemContainer = new UIHBoxContainer();
        itemContainer.DesiredSize = new Vector2(280, 32);

        // Item icon placeholder
        UIVisual itemIcon = UIVisual.FromTexture(
            _shopTexture!,
            new Rectangle(0, 32, 28, 28)
        );
        itemIcon.SetDesiredSize();
        itemContainer.AddChild(itemIcon);

        // Item name
        UIVisual nameText = UIVisual.FromText(itemName, _font!, Color.Black);
        nameText.DesiredSize = new Vector2(150, 16);
        nameText.AlignmentPoint = Alignment.MiddleLeft;
        nameText.Padding = new Vector4(15, 0, 2, 0);
        nameText.SetDesiredSize();
        itemContainer.AddChild(nameText);

        // Price
        UIVisual priceText = UIVisual.FromText($"${price}", _font!, new Color(255, 255, 255));
        priceText.DesiredSize = new Vector2(70, 16);
        priceText.AlignmentPoint = Alignment.MiddleLeft;
        itemContainer.AddChild(priceText);

        // UIElement for buy or sold text
        UIContainer contain = new UIContainer();
        contain.DesiredSize = new Vector2(30, 32);
        contain.AlignmentPoint = Alignment.MiddleRight;

        // Buy button
        UIControl buyButton = new UIControl();
        buyButton.DesiredSize = new Vector2(30, 32);
        buyButton.AlignmentPoint = Alignment.MiddleRight;
        buyButton.Padding = new Vector4(0, 0, 0, 2);
        
        UIVisual buyButtonBg = UIVisual.FromTexture(
            _buyButtonTexture!,
            new Rectangle(0, 0, 30, 22)
        );
        buyButtonBg.SetDesiredSize();

        // Sold Text
        UIVisual soldText = UIVisual.FromText("SOLD", _font!, Color.Red);
        soldText.AlignmentPoint = Alignment.MiddleCenter;
        soldText.SetDesiredSize();
        contain.AddChild(soldText);
        soldText.IsVisible = false;
        
        // Set initial state based on affordability
        if (_coins >= price)
        {
            buyButtonBg.Tint = Color.LightGreen;
        }
        else
        {
            buyButtonBg.SourceRect = new Rectangle(64, 0, 30, 22);
            buyButtonBg.Tint = Color.White;
        }
        
        // UIVisual buyButtonText = UIVisual.FromText("+", _font!, Color.DarkGreen);
        // buyButtonText.AlignmentPoint = Alignment.MiddleCenter;
        // buyButtonText.SetDesiredSize();
        
        buyButton.AddChild(buyButtonBg);
        // buyButton.AddChild(buyButtonText);
        contain.AddChild(buyButton);

        buyButton.HoverEntered += (control) => OnBuyButtonHover(control, buyButtonBg, price);
        buyButton.HoverExited += (control) => OnBuyButtonUnhover(control, buyButtonBg, price);
        buyButton.Pressed += (control) => OnBuyButtonPressed(control, buyButtonBg, price);
        buyButton.Released += (control) => OnBuyButtonReleased(control, buyButtonBg, index, itemName, price);

        itemContainer.AddChild(contain);

        item.Container = itemContainer;
        item.ItemName = itemName;
        item.Price = price;
        item.Icon = itemIcon;
        item.NameText = nameText;
        item.PriceText = priceText;
        item.BuyButton = buyButton;
        item.BuyButtonBg = buyButtonBg;
        item.SoldText = soldText;

        return item;
    }

    #region Button Events
    private void OnBuyButtonHover(UIControl control, UIVisual buttonBg, int price)
    {
        if (buttonBg != null && _coins >= price)
            buttonBg.Tint = Color.Gray;
    }

    private void OnBuyButtonUnhover(UIControl control, UIVisual buttonBg, int price)
    {
        if (buttonBg != null)
        {
            if (_coins >= price)
            {
                buttonBg.Tint = Color.White;
                //buttonBg.SourceRect = new Rectangle(0, 0, 30, 22);
            }
            else
            {
                buttonBg.SourceRect = new Rectangle(64, 0, 30, 22);
                buttonBg.Tint = Color.White;
            }
        }
    }

    private void OnBuyButtonPressed(UIControl control, UIVisual buttonBg, int price)
    {
        if (buttonBg != null && _coins >= price)
            buttonBg.SourceRect = new Rectangle(32, 0, 30, 20);
    }

    private void OnBuyButtonReleased(UIControl control, UIVisual buttonBg, int itemIndex, string itemName, int price)
    {
        if (_coins >= price)
        {
            if (buttonBg != null)
                buttonBg.SourceRect = new Rectangle(64, 0, 30, 22);
            
            OnPurchased(itemIndex, itemName, price, control);
        }
    }

    private void OnExitButtonHover(UIControl control)
    {
        if (_exitButtonVisual != null)
            _exitButtonVisual.Tint = Color.LightGray;
    }

    private void OnExitButtonUnhover(UIControl control)
    {
        if (_exitButtonVisual != null)
            _exitButtonVisual.Tint = Color.White;
    }

    private void OnExitButtonClicked(UIControl control)
    {
        // Hide the shop
        _owner?.CloseShop();
    }

    public void UpdateAllButtonStates()
    {
        for (int i = 0; i < _shopItems.Length; i++)
        {   
            if (_shopItems[i].BuyButtonBg == null) continue;

            _shopItems[i].BuyButton!.IsVisible = true;
            _shopItems[i].SoldText!.IsVisible = false;
            if (_coins >= _shopItems[i].Price)
            {
                _shopItems[i].BuyButtonBg!.SourceRect = new Rectangle(0, 0, 30, 22);
                _shopItems[i].BuyButtonBg!.Tint = Color.LightGreen;
            }
            else
            {
                _shopItems[i].BuyButtonBg!.SourceRect = new Rectangle(64, 0, 30, 22);
                _shopItems[i].BuyButtonBg!.Tint = Color.White;
            }
            
        }
    }

    private void OnPurchased(int itemIndex, string itemName, int price, UIControl buyButton)
    {
        if (_coins >= price && IsVisible)
        {
            // Use the actual item name from the shop item, not the parameter
            string actualItemName = _shopItems[itemIndex].ItemName;
            OnItemPurchased?.Invoke(actualItemName, price);

            // Update all other button states based on new coin amount
            UpdateAllButtonStates();
            
            // TODO: Give item to player
            _owner?.Inventory.AddItemByName(actualItemName);

            if (_owner != null)
                _owner.Stats.Coins -= price;
            
            System.Console.WriteLine($"Purchased: {actualItemName} for ${price}. Remaining coins: {_coins}");
        }
    }
    public static string GetDefaultSavePath(string gameName)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string gameFolder = Path.Combine(appData, gameName);
        return Path.Combine(gameFolder, "inventory_save.json");
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Shows the shop overlay
    /// </summary>
    public void Show()
    {
        this.IsVisible = true;
        UpdateAllButtonStates();
    }

    /// <summary>
    /// Hides the shop overlay
    /// </summary>
    public void Hide()
    {
        this.IsVisible = false;
    }

    public void OnCoinsChanged(int coins)
    {
        _coins = coins;
        UpdateAllButtonStates();
    }
    #endregion

    private class ShopItem
    {
        public UIHBoxContainer? Container { get; set; }
        public string ItemName { get; set; } = "";
        public int Price { get; set; }
        public UIVisual? Icon { get; set; }
        public UIVisual? NameText { get; set; }
        public UIVisual? PriceText { get; set; }
        public UIControl? BuyButton { get; set; }
        public UIVisual? BuyButtonBg { get; set; }
        public UIVisual? SoldText { get; set; }
    }
} 