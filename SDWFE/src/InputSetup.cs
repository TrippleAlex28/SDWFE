using Engine.Input;
using Engine.Input.Binding.Bindings;
using Microsoft.Xna.Framework.Input;

namespace SDWFE;

public static class InputSetup
{
    #region Gameplay
    
    public const string ACTION_MOVE_UP = "MoveUp";
    public const string ACTION_MOVE_DOWN = "MoveDown";
    public const string ACTION_MOVE_LEFT = "MoveLeft";
    public const string ACTION_MOVE_RIGHT = "MoveRight";
    public const string ACTION_LEAP = "Dash";

    public const string ACTION_WEAPON_1 = "Weapon1";
    public const string ACTION_WEAPON_2 = "Weapon2";
    public const string ACTION_WEAPON_SWITCH = "WeaponSwitch";
    
    public const string ACTION_HOTBAR_1 = "Hotbar1";
    public const string ACTION_HOTBAR_2 = "Hotbar2";
    public const string ACTION_HOTBAR_3 = "Hotbar3";
    public const string ACTION_HOTBAR_4 = "Hotbar4";
    public const string ACTION_HOTBAR_5 = "Hotbar5";
    public const string ACTION_HOTBAR_LEFT = "HotbarLeft";
    public const string ACTION_HOTBAR_RIGHT = "HotbarRight";

    public const string ACTION_INVENTORY = "Inventory";
    
    public const string ACTION_SHOOT = "Shoot";
    public const string ACTION_USE = "Use";
    public const string ACTION_INTERACT = "Interact";
    
    public const string ACTION_PAUSE = "Pause";
    public const string ACTION_DIALOGUE = "Dialogue";
    
    #endregion
    
    #region UI
    
    public const string ACTION_UI_SELECT = "UISelect"; // NEVER MODIFY THIS, THE RAW STRING IS USED IN ENGINE FOR UI
    public const string ACTION_UI_CANCEL = "UICancel";
    public const string ACTION_UI_NAVIGATE_UP = "UINavigateUp";
    public const string ACTION_UI_NAVIGATE_DOWN = "UINavigateDown";
    public const string ACTION_UI_NAVIGATE_LEFT = "UINavigateLeft";
    public const string ACTION_UI_NAVIGATE_RIGHT = "UINavigateRight";

    public const string ACTION_UI_INVENTORY = "UIInventory";
    
    #endregion
    
    public const string PROFILE_GAMEPLAY = "Gameplay";
    public const string PROFILE_UI = "UI";

    public static void Initialize(string defaultProfile = PROFILE_UI)
    {
        CreateGameplayProfile();
        CreateUIProfile();
        
        InputManager.Instance.SetActiveProfile(defaultProfile);
    }

    private static void CreateGameplayProfile()
    {
        var gameplayProfile = new InputProfile(PROFILE_GAMEPLAY);

        var moveUp = new InputAction(ACTION_MOVE_UP)
            .AddBinding(new KeyboardBinding(Keys.W))
            .AddBinding(new KeyboardBinding(Keys.Up))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickUp));
        var moveDown = new InputAction(ACTION_MOVE_DOWN)
            .AddBinding(new KeyboardBinding(Keys.S))
            .AddBinding(new KeyboardBinding(Keys.Down))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickDown));
        var moveLeft = new InputAction(ACTION_MOVE_LEFT)
            .AddBinding(new KeyboardBinding(Keys.A))
            .AddBinding(new KeyboardBinding(Keys.Left))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickLeft));
        var moveRight = new InputAction(ACTION_MOVE_RIGHT)
            .AddBinding(new KeyboardBinding(Keys.D))
            .AddBinding(new KeyboardBinding(Keys.Right))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickRight));
        var leap = new InputAction(ACTION_LEAP)
            .AddBinding(new KeyboardBinding(Keys.Space))
            .AddBinding(new GamePadButtonBinding(Buttons.A));

        var weapon1 = new InputAction(ACTION_WEAPON_1)
            .AddBinding(new KeyboardBinding(Keys.D1));
        var weapon2 = new InputAction(ACTION_WEAPON_2)
            .AddBinding(new KeyboardBinding(Keys.D2));
        var weaponSwitch = new InputAction(ACTION_WEAPON_SWITCH)
            .AddBinding(new GamePadButtonBinding(Buttons.Y));
        
        var hotbar1 = new InputAction(ACTION_HOTBAR_1)
            .AddBinding(new KeyboardBinding(Keys.D3));
        var hotbar2 = new InputAction(ACTION_HOTBAR_2)
            .AddBinding(new KeyboardBinding(Keys.D4));
        var hotbar3 = new InputAction(ACTION_HOTBAR_3)
            .AddBinding(new KeyboardBinding(Keys.D5));
        var hotbar4 = new InputAction(ACTION_HOTBAR_4)
            .AddBinding(new KeyboardBinding(Keys.D6));
        var hotbar5 = new InputAction(ACTION_HOTBAR_5)
            .AddBinding(new KeyboardBinding(Keys.D7));
        var hotbarLeft = new InputAction(ACTION_HOTBAR_LEFT)
            .AddBinding(new MouseButtonBinding(MouseButtonBinding.MouseButton.WheelDown))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftShoulder));
        var hotbarRight = new InputAction(ACTION_HOTBAR_RIGHT)
            .AddBinding(new MouseButtonBinding(MouseButtonBinding.MouseButton.WheelUp))
            .AddBinding(new GamePadButtonBinding(Buttons.RightShoulder));

        var inventory = new InputAction(ACTION_INVENTORY)
            .AddBinding(new KeyboardBinding(Keys.Tab))
            .AddBinding(new GamePadButtonBinding(Buttons.DPadUp));
        
        var shoot = new InputAction(ACTION_SHOOT)
            .AddBinding(new MouseButtonBinding(MouseButtonBinding.MouseButton.Left))
            .AddBinding(new GamePadButtonBinding(Buttons.RightTrigger));
        var use = new InputAction(ACTION_USE)
            .AddBinding(new KeyboardBinding(Keys.E))
            .AddBinding(new GamePadButtonBinding(Buttons.X));
        var interact = new InputAction(ACTION_INTERACT)
            .AddBinding(new KeyboardBinding(Keys.F))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftTrigger));

        var pause = new InputAction(ACTION_PAUSE)
            .AddBinding(new KeyboardBinding(Keys.Escape))
            .AddBinding(new GamePadButtonBinding(Buttons.Start));
        
        var dialogue = new InputAction(ACTION_DIALOGUE)
            .AddBinding(new KeyboardBinding(Keys.T))
            .AddBinding(new GamePadButtonBinding(Buttons.Back));
        
        gameplayProfile.RegisterAction(moveUp);
        gameplayProfile.RegisterAction(moveDown);
        gameplayProfile.RegisterAction(moveLeft);
        gameplayProfile.RegisterAction(moveRight);
        gameplayProfile.RegisterAction(leap);
        
        gameplayProfile.RegisterAction(weapon1);
        gameplayProfile.RegisterAction(weapon2);
        gameplayProfile.RegisterAction(weaponSwitch);
        
        gameplayProfile.RegisterAction(hotbar1);
        gameplayProfile.RegisterAction(hotbar2);
        gameplayProfile.RegisterAction(hotbar3);
        gameplayProfile.RegisterAction(hotbar4);
        gameplayProfile.RegisterAction(hotbar5);
        gameplayProfile.RegisterAction(hotbarLeft);
        gameplayProfile.RegisterAction(hotbarRight);
        
        gameplayProfile.RegisterAction(inventory);
        
        gameplayProfile.RegisterAction(shoot);
        gameplayProfile.RegisterAction(use);
        gameplayProfile.RegisterAction(interact);
        
        gameplayProfile.RegisterAction(pause);
        gameplayProfile.RegisterAction(dialogue);
        
        // UI actions needed for shop and other UI during gameplay
        var uiSelect = new InputAction(ACTION_UI_SELECT)
            .AddBinding(new MouseButtonBinding(MouseButtonBinding.MouseButton.Left))
            .AddBinding(new KeyboardBinding(Keys.Space))
            .AddBinding(new GamePadButtonBinding(Buttons.A));
        gameplayProfile.RegisterAction(uiSelect);
        
        InputManager.Instance.RegisterProfile(gameplayProfile);
    }

    private static void CreateUIProfile()
    {
        var uiProfile = new InputProfile(PROFILE_UI);

        var select = new InputAction(ACTION_UI_SELECT)
            .AddBinding(new MouseButtonBinding(MouseButtonBinding.MouseButton.Left))
            .AddBinding(new KeyboardBinding(Keys.Space))
            .AddBinding(new GamePadButtonBinding(Buttons.A));
        
        var cancel = new InputAction(ACTION_UI_CANCEL)
            .AddBinding(new KeyboardBinding(Keys.Escape))
            .AddBinding(new GamePadButtonBinding(Buttons.B));
        
        var navigateUp = new InputAction(ACTION_UI_NAVIGATE_UP)
            .AddBinding(new KeyboardBinding(Keys.W))
            .AddBinding(new KeyboardBinding(Keys.Up))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickUp));
        
        var navigateDown = new InputAction(ACTION_UI_NAVIGATE_DOWN)
            .AddBinding(new KeyboardBinding(Keys.S))
            .AddBinding(new KeyboardBinding(Keys.Down))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickDown));
        
        var navigateLeft = new InputAction(ACTION_UI_NAVIGATE_LEFT)
            .AddBinding(new KeyboardBinding(Keys.A))
            .AddBinding(new KeyboardBinding(Keys.Left))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickLeft));
        
        var navigateRight = new InputAction(ACTION_UI_NAVIGATE_RIGHT)
            .AddBinding(new KeyboardBinding(Keys.D))
            .AddBinding(new KeyboardBinding(Keys.Right))
            .AddBinding(new GamePadButtonBinding(Buttons.LeftThumbstickRight));

        var inventory = new InputAction(ACTION_UI_INVENTORY)
            .AddBinding(new KeyboardBinding(Keys.T))
            .AddBinding(new GamePadButtonBinding(Buttons.DPadUp));
        
        uiProfile.RegisterAction(select);
        uiProfile.RegisterAction(cancel);
        uiProfile.RegisterAction(navigateUp);
        uiProfile.RegisterAction(navigateDown);
        uiProfile.RegisterAction(navigateLeft);
        uiProfile.RegisterAction(navigateRight);

        uiProfile.RegisterAction(inventory);
        
        InputManager.Instance.RegisterProfile(uiProfile);
    }
}