using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Engine.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using Engine.Input;
using Engine.Sprite;
using Engine.Hitbox;

#nullable enable
public class RoomDoor : GameObject
{
    public Rectangle _hitBox;

    private int _tileSize = 32;
    private Texture2D _spriteSheet;
    private AnimatedSprite? _animatedSprite;
    private StaticHitbox? _staticHitbox;
    private readonly HitboxManager _hitboxManager;

    public RoomDoor(Vector2 globalPosition, HitboxManager hitboxManager)
    {
        this._spriteSheet = ExtendedGame.AssetManager.LoadTexture("TM_Door_Anim", "Tilemap/");
        this.GlobalPosition = globalPosition;
        this._hitboxManager = hitboxManager;

        _staticHitbox = new StaticHitbox(new Rectangle((int)globalPosition.X, (int)globalPosition.Y + 24, _tileSize, 8));
        _staticHitbox.BlocksLayers = HitboxLayer.All;
        _hitboxManager.AddStatic(_staticHitbox);

        _animatedSprite = new AnimatedSprite(_spriteSheet, _tileSize, _tileSize, 200f, false, false);
        _animatedSprite.BaseDrawLayer = ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, 32)) - 0.0001f;

        _animatedSprite.AnimationCompleted += () =>
        {
            disableHitbox();
        };
        this.AddChild(_animatedSprite);
    }
    public void Open()
    {
        _animatedSprite?.Play();
    }

    public void Reset()
    {
        _animatedSprite?.Reset();
        _staticHitbox.IsEnabled = true;
    }

    private void disableHitbox()
    {
        _staticHitbox.IsEnabled = false;
    }
}