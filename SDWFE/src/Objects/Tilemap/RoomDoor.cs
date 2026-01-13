using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Engine.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using Engine.Input;
using Engine.Sprite;
using Engine.Hitbox;
using SDWFE.Objects;

#nullable enable
public class RoomDoor : GameObject
{
    public override uint TypeId => (uint)NetObjects.RoomDoor;
    
    public Rectangle _hitBox;

    private int _tileSize = 32;
    private Texture2D? _spriteSheet;
    private AnimatedSprite? _animatedSprite;
    private StaticHitbox? _staticHitbox;
    private HitboxManager? _hitboxManager;
    
    private bool _isOpen = false;
    private bool _hasPlayedOpenAnimation = false;
    
    /// <summary>
    /// Parameterless constructor for network object creation.
    /// </summary>
    public RoomDoor()
    {
        this.ReplicatesOverNetwork = true;
        
        RegisterProperty(
            nameof(_isOpen),
            () => _isOpen,
            (v) => {
                bool wasOpen = _isOpen;
                _isOpen = v;
                // Trigger animation when state changes from closed to open
                if (_isOpen && !wasOpen && !_hasPlayedOpenAnimation)
                {
                    PlayOpenAnimation();
                }
            }
        );
    }

    public RoomDoor(Vector2 globalPosition, HitboxManager hitboxManager) : this()
    {
        this._spriteSheet = ExtendedGame.AssetManager.LoadTexture("TM_Door_Anim", "Tilemap/");
        this.GlobalPosition = globalPosition;
        this._hitboxManager = hitboxManager;

        _staticHitbox = new StaticHitbox(new Rectangle((int)globalPosition.X, (int)globalPosition.Y + 24, _tileSize, 8));
        _staticHitbox.BlocksLayers = HitboxLayer.All;
        _hitboxManager.AddStatic(_staticHitbox);

        _animatedSprite = new AnimatedSprite(_spriteSheet, _tileSize, _tileSize, false, false);
        _animatedSprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 32)) - 0.0001f;

        _animatedSprite.AnimationCompleted += () =>
        {
            disableHitbox();
        };
        this.AddChild(_animatedSprite);
    }
    
    protected override void EnterSelf()
    {
        base.EnterSelf();
        
        // Initialize sprite for network-created doors (parameterless constructor)
        if (_spriteSheet == null)
        {
            _spriteSheet = ExtendedGame.AssetManager.LoadTexture("TM_Door_Anim", "Tilemap/");
            _animatedSprite = new AnimatedSprite(_spriteSheet, _tileSize, _tileSize, false, false);
            _animatedSprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 32)) - 0.0001f;
            _animatedSprite.AnimationCompleted += () =>
            {
                disableHitbox();
            };
            this.AddChild(_animatedSprite);
        }
        
        // If already open when entering, show final frame
        if (_isOpen)
        {
            _animatedSprite?.SetToLastFrame();
            _hasPlayedOpenAnimation = true;
        }
    }
    
    private void PlayOpenAnimation()
    {
        _animatedSprite?.Play();
        _hasPlayedOpenAnimation = true;
    }
    
    public void Open()
    {
        _isOpen = true;
        if (!_hasPlayedOpenAnimation)
        {
            PlayOpenAnimation();
        }
    }

    public void Reset()
    {
        _animatedSprite?.Reset();
        _staticHitbox!.IsEnabled = true;
        _isOpen = false;
        _hasPlayedOpenAnimation = false;
    }

    private void disableHitbox()
    {
        if (_staticHitbox != null)
            _staticHitbox.IsEnabled = false;
    }
}