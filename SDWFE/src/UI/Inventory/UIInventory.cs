using System;
using System.Collections.Generic;
using Engine;
using Engine.Input;
using Engine.UI;
using Engine.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Inventory;

namespace SDWFE.UI.Inventory2;

public class UIInventory : UIContainer
{
    private PlayerInventory _inventory;

    // Always visible components
    private UIHotbar _hotbar;
    private UIWeapons _weapons;

    // Menu components
    private UIVisual _menuBackground;
    private UIHBoxContainer _scrollBoxContainer;
    private UIVBoxContainer _inventorySection;
    private UIVBoxContainer _vaultSection;
    private UIScrollContainer _inventoryScroll;
    private UIScrollContainer _vaultScroll;
    private UIVisual _vaultPlaceholder;
    private UIVisual _inventoryLabel;
    private UIVisual _vaultLabel;

    private List<UIHBoxContainer> _inventoryRows = new();
    private List<UIInventoryItemSlot> _inventorySlots = new();
    private List<UIHBoxContainer> _vaultRows = new();
    private List<UIInventoryItemSlot> _vaultSlots = new();

    private IUIFocusableSlot? _focusedSlot = null;
    
    private Texture2D _slotSheet;
    private Rectangle _slotSheetRect;
    private Rectangle _selectedSlotSheetRect;

    private bool _isMenuOpen = false;

    private const float MENU_WIDTH_PERCENT = 70f;
    private const float MENU_HEIGHT_PERCENT = 60f;
    private const float SCROLL_BOX_SPACING = 8f;
    private const float LABEL_HEIGHT = 24f;
    private const float MENU_SLOT_SIZE = 32f;
    private const float SLOT_SPACING = 4f;
    private const int GRID_COLUMNS = 3;

    public bool IsMenuOpen => _isMenuOpen;

    public UIInventory(PlayerInventory inventory)
    {
        _inventory = inventory;
        _inventory.OnInventoryChanged += HandleInventoryChange;

        _slotSheet = ExtendedGame.AssetManager.LoadTexture("inventorySheet", "UI/");
        _slotSheetRect = new Rectangle(0, 32, 28, 28);
        _selectedSlotSheetRect = new Rectangle(0, 64, 28, 28);

        AlignmentPoint = Alignment.TopLeft;
        DesiredSize = UIExtensionMethods.ScreenPercent(100, 100);

        // Create always-visible components
        _hotbar = new UIHotbar(_inventory);
        AddChild(_hotbar);
        foreach (var s1 in _hotbar.Slots)
        {
            s1.Clicked += HandleSlotClicked;
            s1.OnFocusChange += (focused) =>
            {
                if (!focused)
                {
                    if (_focusedSlot == s1) 
                        UnfocusSlot();
                    return;
                }
                
                UnfocusSlot();
                FocusSlot(s1);
            };
        }

        _weapons = new UIWeapons(_inventory);
        AddChild(_weapons);
        foreach (var s2 in _weapons.Slots)
        {
            s2.Clicked += HandleSlotClicked;
            s2.OnFocusChange += (focused) =>
            {
                if (!focused)
                {
                    if (_focusedSlot == s2) UnfocusSlot();
                    return;
                }
                UnfocusSlot();
                FocusSlot(s2);
            };
        }

        // Create menu components
        CreateMenuComponents();

        // Start with menu closed
        SetMenuVisibility(false);
    }

    protected override void EnterSelf()
    {
        HandleInventoryChange();
    }

    private void CreateMenuComponents()
    {
        Vector2 menuSize = UIExtensionMethods.ScreenPercent(MENU_WIDTH_PERCENT, MENU_HEIGHT_PERCENT);

        // Menu background
        _menuBackground = UIVisual.FromColor(new Color(15, 15, 15, 230));
        _menuBackground.AlignmentPoint = Alignment.MiddleCenter;
        _menuBackground.DesiredSize = menuSize;
        _menuBackground.Padding = new Vector4(4);
        AddChild(_menuBackground);

        // Container for side-by-side scroll boxes
        _scrollBoxContainer = new UIHBoxContainer()
        {
            AlignmentPoint = Alignment.MiddleCenter,
            DesiredSize = Vector2.Zero,
            Spacing = SCROLL_BOX_SPACING
        };
        _menuBackground.AddChild(_scrollBoxContainer);

        // Left side - Player Inventory
        CreateInventorySection();

        // Right side - Vault
        CreateVaultSection();
        
        // Bottom labels
        var labelContainer = new UIVBoxContainer()
        {
            AlignmentPoint = Alignment.BottomMiddle,
            DesiredSize = new Vector2(0, 16),
            Margin = new Vector4(4, 0, 0, 0)
        };
        _menuBackground.AddChild(labelContainer);
        
        var eLabel = UIVisual.FromText("E - Switch Inv & Inventory", Resources.GetFont(Resources.UPHEAVEL_FONTNAME, 12), Color.White);
        labelContainer.AddChild(eLabel);
    }

    private void CreateInventorySection()
    {
        _inventorySection = new UIVBoxContainer()
        {
            DesiredSize = Vector2.Zero,
            MinSize = new Vector2(0, 0),
            MaxSize = new Vector2(float.MaxValue, float.MaxValue),
            AlignmentPoint = Alignment.MiddleCenter,
            Spacing = 2f
        };
        _scrollBoxContainer.AddChild(_inventorySection);

        // Inventory label
        _inventoryLabel = UIVisual.FromText("INVENTORY", Resources.TextFont, Color.White);
        _inventoryLabel.AlignmentPoint = Alignment.TopMiddle;
        _inventoryLabel.DesiredSize = new Vector2(0, 0);
        _inventorySection.AddChild(_inventoryLabel);

        // Inventory scroll container
        _inventoryScroll = new UIScrollContainer(isHorizontal: false)
        {
            DesiredSize = Vector2.Zero,
            MinSize = new Vector2(0, 100),
            MaxSize = new Vector2(float.MaxValue, float.MaxValue),
            Margin = new Vector4(8, 4, 8, 4),
            Spacing = SLOT_SPACING,
            AlignmentPoint = Alignment.MiddleCenter,
            ChildAlignment = Alignment.TopMiddle,
        };
        _inventorySection.AddChild(_inventoryScroll);

        PopulateInventoryScroll();
    }

    private void CreateVaultSection()
    {
        _vaultSection = new UIVBoxContainer()
        {
            DesiredSize = Vector2.Zero,
            MinSize = new Vector2(0, 0),
            MaxSize = new Vector2(float.MaxValue, float.MaxValue),
            AlignmentPoint = Alignment.MiddleCenter,
            Spacing = 2f
        };
        _scrollBoxContainer.AddChild(_vaultSection);

        // Vault label
        _vaultLabel = UIVisual.FromText("VAULT", Resources.TextFont, Color.White);
        _vaultLabel.AlignmentPoint = Alignment.TopMiddle;
        _vaultLabel.DesiredSize = new Vector2(0, LABEL_HEIGHT / 2);
        _vaultSection.AddChild(_vaultLabel);

        // Vault scroll container
        _vaultScroll = new UIScrollContainer(isHorizontal: false)
        {
            DesiredSize = Vector2.Zero,
            MinSize = new Vector2(0, 100),
            MaxSize = new Vector2(float.MaxValue, float.MaxValue),
            Margin = new Vector4(8, 4, 8, 4),
            Spacing = SLOT_SPACING,
            AlignmentPoint = Alignment.MiddleCenter,
            ChildAlignment = Alignment.TopMiddle,
        };
        _vaultSection.AddChild(_vaultScroll);
        
        // Vault placeholder
        _vaultPlaceholder = UIVisual.FromText(
            "Vault Not Accessible\n\nFind a vault terminal\nto access your storage",
            Resources.TextFont,
            new Color(180, 180, 180, 255)
        );
        _vaultPlaceholder.AlignmentPoint = Alignment.MiddleCenter;
        _vaultPlaceholder.Padding = new Vector4(16, 16, 16, 16);
        _vaultSection.AddChild(_vaultPlaceholder);

        PopulateVaultScroll();
    }

    private void PopulateInventoryScroll()
    {
        // Clears existing rows
        foreach (var row in _inventoryRows)
        {
            _inventoryScroll.RemoveChild(row);
        }
        _inventoryRows.Clear();
        _inventorySlots.Clear();

        int totalSlots = _inventory.Inventory.Length;
        int currentColumn = 0;
        UIHBoxContainer? currentRow = null;
        
        for (int i = 0; i < totalSlots; i++)
        {
            // Create new row when starting or after filling GRID_COLUMNS
            if (currentColumn == 0)
            {
                currentRow = new UIHBoxContainer()
                {
                    DesiredSize = Vector2.Zero,
                    MinSize = new Vector2(0, MENU_SLOT_SIZE),
                    MaxSize = new Vector2(float.MaxValue, MENU_SLOT_SIZE),
                    Spacing = SLOT_SPACING,
                    AlignmentPoint = Alignment.TopMiddle
                };
                _inventoryScroll.AddChild(currentRow);
                _inventoryRows.Add(currentRow);
            }
            
            var slot = new UIInventoryItemSlot(
                _inventory.Inventory[i],
                i,
                MENU_SLOT_SIZE,
                _slotSheet,
                _slotSheetRect,
                _selectedSlotSheetRect,
                _inventory,
                isVault: false
            );
            slot.Clicked += HandleSlotClicked;
            slot.OnFocusChange += (focused) =>
            {
                if (!focused)
                {
                    if (_focusedSlot == slot) UnfocusSlot();
                    return;
                }

                UnfocusSlot();
                FocusSlot(slot);
            };
            _inventorySlots.Add(slot);
            currentRow!.AddChild(slot);

            currentColumn++;
            if (currentColumn >= GRID_COLUMNS)
            {
                currentColumn = 0;
            }
        }
    }

    private void PopulateVaultScroll()
    {
        foreach (var row in _vaultRows)
        {
            _vaultScroll.RemoveChild(row);
        }
        _vaultRows.Clear();
        _vaultSlots.Clear();

        if (!_inventory.Vault.IsAccessible)
            return;

        // Create grid layout with rows
        int totalSlots = _inventory.Vault.Slots.Length;
        int currentColumn = 0;
        UIHBoxContainer? currentRow = null;
        
        for (int i = 0; i < totalSlots; i++)
        {
            // Create new row when starting or after filling GRID_COLUMNS
            if (currentColumn == 0)
            {
                currentRow = new UIHBoxContainer()
                {
                    DesiredSize = Vector2.Zero,
                    MinSize = new Vector2(0, MENU_SLOT_SIZE),
                    MaxSize = new Vector2(float.MaxValue, MENU_SLOT_SIZE),
                    Spacing = SLOT_SPACING,
                    AlignmentPoint = Alignment.TopMiddle
                };
                _vaultScroll.AddChild(currentRow);
                _vaultRows.Add(currentRow);
            }

            var slot = new UIInventoryItemSlot(
                _inventory.Vault.Slots[i],
                i,
                MENU_SLOT_SIZE,
                _slotSheet,
                _slotSheetRect,
                _selectedSlotSheetRect,
                _inventory,
                isVault: true
            );
            slot.Clicked += HandleSlotClicked;
            slot.OnFocusChange += (focused) =>
            {
                if (!focused)
                {
                    if (_focusedSlot == slot)
                        UnfocusSlot();
                    return;
                }

                UnfocusSlot();
                FocusSlot(slot);
            };
            _vaultSlots.Add(slot);
            currentRow!.AddChild(slot);

            currentColumn++;
            if (currentColumn >= GRID_COLUMNS)
            {
                currentColumn = 0;
            }
        }
    }

    public void ToggleMenu()
    {
        if (_isMenuOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        if (!_isMenuOpen)
        {
            _isMenuOpen = true;
            SetMenuVisibility(true);
            UpdateVaultVisibility();
            RefreshInventoryDisplay();
            InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_UI);
        }
    }

    public void CloseMenu()
    {
        if (_isMenuOpen)
        {
            _isMenuOpen = false;
            SetMenuVisibility(false);
            InputManager.Instance.SetActiveProfile(InputSetup.PROFILE_GAMEPLAY);

            _focusedSlot?.SetFocused(false);
            _focusedSlot = null;
        }
    }

    private void SetMenuVisibility(bool visible)
    {
        _menuBackground.IsVisible = visible;
    }

    private void UpdateVaultVisibility()
    {
        bool vaultAccessible = _inventory.Vault.IsAccessible;
        _vaultScroll.IsVisible = vaultAccessible;
        _vaultPlaceholder.IsVisible = !vaultAccessible;

        if (vaultAccessible)
        {
            PopulateVaultScroll();
        }
    }

    private void RefreshInventoryDisplay()
    {
        for (int i = 0; i < _inventorySlots.Count; i++)
        {
            _inventorySlots[i].Refresh();
        }

        for (int i = 0; i < _vaultSlots.Count; i++)
        {
            _vaultSlots[i].Refresh();
        }
    }

    private void HandleInventoryChange()
    {
        if (_isMenuOpen)
        {
            RefreshInventoryDisplay();
        }
    }

    private void HandleSlotClicked(IUIFocusableSlot clicked)
    {
        if (_focusedSlot == clicked)
        {
            UnfocusSlot();
            return;
        }

        if (_focusedSlot != null)
        {
            _inventory.SwapSlots(ResolveSlot(_focusedSlot), ResolveSlot(clicked));

            UnfocusSlot();
            return;
        }

        if (ResolveSlot(clicked).IsEmpty())
            return;

        UnfocusSlot();
        clicked.SetFocused(true);
    }
    
    private void UnfocusSlot()
    {
        _focusedSlot?.Unfocus();
        _focusedSlot = null;
    }

    private void FocusSlot(IUIFocusableSlot slot)
    {
        _focusedSlot = slot;
    }

    private void SwapFocusToWeapon(int idx)
    {
        if (_focusedSlot == null || _focusedSlot.Slot.IsEmpty())
            return;

        _inventory.SwapSlots(ResolveSlot(_focusedSlot), _inventory.WeaponSlots[idx]);
        
        UnfocusSlot();
    }

    private void SwapFocusToHotbar(int idx)
    {
        if (_focusedSlot == null || _focusedSlot.Slot.IsEmpty())
            return;
        
        _inventory.SwapSlots(ResolveSlot(_focusedSlot), _inventory.Hotbar[idx]);
        
        UnfocusSlot();
    }

    /// <summary>
    /// Transfer to/from vault from/to inventory
    /// </summary>
    private void IVSwap()
    {    
        if (_focusedSlot == null || _focusedSlot.Slot.IsEmpty())
            return;

        if (_focusedSlot.Group == UISlotGroup.Vault)
        {
            _inventory.TransferFromVault(_focusedSlot.Slot.Item!.Name, _focusedSlot.Slot.Item.StackSize);
            UnfocusSlot();
        }
        else if (_focusedSlot.Group == UISlotGroup.Inventory)
        {
            if (!_inventory.Vault.IsAccessible)
                return;

            _inventory.TransferToVault(_focusedSlot.Slot.Item!.Name, _focusedSlot.Slot.Item.StackSize);
            
            UnfocusSlot();
        }
    }
    
    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);

        if (_isMenuOpen)
        {
            bool previousVaultState = _vaultScroll.IsVisible;
            bool currentVaultState = _inventory.Vault.IsAccessible;

            if (previousVaultState != currentVaultState)
            {
                UpdateVaultVisibility();
            }
        }

        if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_INVENTORY) || InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_INVENTORY))
        {
            ToggleMenu();
        }

        if (_isMenuOpen)
        {
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_WEAPON_1)) SwapFocusToWeapon(0);
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_WEAPON_2)) SwapFocusToWeapon(1);
        
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_HOTBAR_1)) SwapFocusToHotbar(0);
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_HOTBAR_2)) SwapFocusToHotbar(1);
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_HOTBAR_3)) SwapFocusToHotbar(2);
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_HOTBAR_4)) SwapFocusToHotbar(3);
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_HOTBAR_5)) SwapFocusToHotbar(4);
            
            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_UI_IVSWAP)) IVSwap();
        }
    }
    
    private InventorySlot ResolveSlot(IUIFocusableSlot slot)
    {
        return slot.Group switch
        {
            UISlotGroup.Inventory => _inventory.Inventory[slot.Index],
            UISlotGroup.Vault     => _inventory.Vault.Slots[slot.Index],
            UISlotGroup.Hotbar    => _inventory.Hotbar[slot.Index],
            UISlotGroup.Weapon    => _inventory.WeaponSlots[slot.Index],
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}