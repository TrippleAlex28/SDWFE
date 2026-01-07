
using Engine;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.UI.PlayerData;

public class UIStats : UIContainer
{
    private PlayerStats _stats;

    private UIHBoxContainer _mainHBox;

    private UIContainer _playerPortraitContainer;
    private UIVisual _playerPortraitBg;
    private UIVisual _playerPortrait;

    private UIVBoxContainer _playerInfoContainer;
    private UIContainer _barsContainer;
    private UIVisual _barsBg;
    private UIVisual _playerIndex;
    private UIProgressbar _healthBar;
    private UIProgressbar _staminaBar;

    private UIHBoxContainer _coinsContainer;
    private UIVisual _coinIcon;
    private UIHBoxContainer _coinNumberContainer;

    private Texture2D _barsSheet;
    private Texture2D _portraitSheet;
    private Texture2D _numberSheet;

    private int Scalefactor = 1;

    public UIStats(PlayerStats stats)
    {
        _stats = stats;
        _stats.OnStatsChanged += UpdateStats;

        // Load Textures
        _barsSheet = ExtendedGame.AssetManager.LoadTexture("UI_TopLeft_Menu", "UI/");
        _numberSheet = ExtendedGame.AssetManager.LoadTexture("UI_Numbers", "UI/");
        _portraitSheet = ExtendedGame.AssetManager.LoadTexture("UI_Player_Portret", "UI/");

        // Make The Main HBox
        _mainHBox = new UIHBoxContainer();
        _mainHBox.Margin = new Vector4(10, 10, 10, 10) * Scalefactor;
        _mainHBox.AlignmentPoint = Alignment.TopLeft;
        DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);
        AddChild(_mainHBox);

        // Player Portrait Container
        _playerPortraitContainer = new UIContainer();
        _playerPortraitContainer.MaxSize = new Vector2(32, 32) * Scalefactor;
        _mainHBox.AddChild(_playerPortraitContainer);

        // Player Portrait Background
        _playerPortraitBg = UIVisual.FromTexture(_barsSheet, new Rectangle(64, 0, 32, 32));
        _playerPortraitBg.DesiredSize = new Vector2(32, 32) * Scalefactor;
        _playerPortraitContainer.AddChild(_playerPortraitBg);

        // Player Portrait
        _playerPortrait = UIVisual.FromTexture(_portraitSheet, new Rectangle(0, 0, 32, 32));
        _playerPortrait.DesiredSize = new Vector2(32, 32) * Scalefactor;
        _playerPortraitContainer.AddChild(_playerPortrait);

        // Player Info Container
        _playerInfoContainer = new UIVBoxContainer();
        _playerInfoContainer.DesiredSize = new Vector2(64, 32) * Scalefactor;
        _mainHBox.AddChild(_playerInfoContainer);

        // Bars Container
        _barsContainer = new UIContainer();
        _barsContainer.DesiredSize = new Vector2(64, 24) * Scalefactor;
        _playerInfoContainer.AddChild(_barsContainer);

        // Bars Background 
        _barsBg = UIVisual.FromTexture(_barsSheet, new Rectangle(0, 32, 64, 24));
        _barsBg.DesiredSize = new Vector2(64, 24) * Scalefactor;
        _barsContainer.AddChild(_barsBg);

        // Health Bar - Fill
        var Fill = UIVisual.FromTexture(_barsSheet, new Rectangle(0, 96, 61, 24));
        Fill.DesiredSize = new Vector2(61, 24) * Scalefactor;

        // Health Bar - Progressbar
        _healthBar = new UIProgressbar(
            fill: Fill,
            background: null,
            scale: Scalefactor
        );
        _barsContainer.AddChild(_healthBar);

        // Stamina Bar - Fill
        var StaminaFill = UIVisual.FromTexture(_barsSheet, new Rectangle(0, 64, 46, 24));
        StaminaFill.DesiredSize = new Vector2(46, 24) * Scalefactor;

        // Stamina Bar - Progressbar
        _staminaBar = new UIProgressbar(
            fill: StaminaFill,
            background: null,
            scale: Scalefactor
        );
        _barsContainer.AddChild(_staminaBar);

        // Player Index
        SetPlayerIndex();
        _barsContainer.AddChild(_playerIndex);

        // Coins Container
        _coinsContainer = new UIHBoxContainer();
        _coinsContainer.DesiredSize = new Vector2(64, 8) * Scalefactor;
        _coinsContainer.Margin = new Vector4(1, 1, 0, 0) * Scalefactor;
        _playerInfoContainer.AddChild(_coinsContainer);

        // Coin Icon
        _coinIcon = UIVisual.FromTexture(_barsSheet, new Rectangle(80, 32, 6, 6));
        _coinIcon.DesiredSize = new Vector2(6, 6) * Scalefactor;
        _coinIcon.MaxSize = new Vector2(6, 6) * Scalefactor;
        
        _coinsContainer.AddChild(_coinIcon);

        // Coin Text
        _coinNumberContainer = new UIHBoxContainer();
        _coinNumberContainer.Spacing = 1 * Scalefactor;
        _coinNumberContainer.Margin = new Vector4(2, 1, 0, 0) * Scalefactor;
        _coinsContainer.AddChild(_coinNumberContainer);
    }
    public void SetPlayerIndex()
    {
        int index = MathHelper.Clamp(_stats._ownerIndex, 1, 2);
        switch (index)
        {
            case 1:
                _playerIndex = UIVisual.FromTexture(_barsSheet, new Rectangle(64, 64, 14, 9));
                break;
            case 2:
                _playerIndex = UIVisual.FromTexture(_barsSheet, new Rectangle(64, 32, 14, 9));
                break;
        }
        _playerIndex.DesiredSize = new Vector2(14, 9) * Scalefactor;
    }

    public void SetCoinCount(int count)
    {
        if (_coinNumberContainer == null || _numberSheet == null)
            return;

        // Clear existing number sprites
        _coinNumberContainer.RemoveAllChildren();

        // Convert count to string to get individual digits
        string countStr = count.ToString();

        _coinNumberContainer.DesiredSize = new Vector2(3 * countStr.Length + countStr.Length, 5) * Scalefactor;
        // Create a sprite for each digit
        foreach (char digitChar in countStr)
        {
            int digit = digitChar - '0'; // Convert char to int (0-9)

            // Each number is 3 pixels wide and 5 pixels high, starting at x=0 for digit 0
            Rectangle sourceRect = new Rectangle(digit * 3, 0, 3, 5);

            UIVisual digitSprite = UIVisual.FromTexture(_numberSheet, sourceRect);
            digitSprite.DesiredSize = new Vector2(3, 5) * Scalefactor;
            // Optional: Set Color of the digit
            digitSprite.Tint = new Color(36, 36, 36);
            digitSprite.SetDesiredSize();

            _coinNumberContainer.AddChild(digitSprite);
        }
    }

    public void UpdateStats()
    {
        if (_stats == null)
            return;

        // Update Health Bar
        float healthPercentage = _stats.CurrentHealth / _stats.MaxHealth;
        _healthBar.SetProgressInstant(healthPercentage);

        // Update Stamina Bar
        float staminaPercentage = _stats.CurrentStamina / _stats.MaxStamina;
        _staminaBar.SetProgressInstant(staminaPercentage);

        // Update Coins
        SetCoinCount(_stats.Coins);

    }
}