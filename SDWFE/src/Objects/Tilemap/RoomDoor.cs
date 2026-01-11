using System.Collections.Generic;
using Engine;
using Microsoft.Xna.Framework;
using Engine.UI;
using Microsoft.Xna.Framework.Graphics;
using System;
using Engine.Input;
using Engine.Sprite;
using Engine.Hitbox;

public class RoomDoor : GameObject
{
    public Rectangle _hitBox;

    private int _tileSize = 32;
    private Texture2D _spriteSheet;
    private AnimatedSprite? _animatedSprite;
    private StaticHitbox? _staticHitbox;
    private readonly HitboxManager _hitboxManager;

    public RoomDoor(Texture2D spriteSheet, Vector2 globalPosition, int tileSize, HitboxManager hitboxManager)
    {
        this._tileSize = tileSize;
        this._spriteSheet = spriteSheet;
        this.GlobalPosition = globalPosition;
        this._hitboxManager = hitboxManager;

        _staticHitbox = new StaticHitbox(new Rectangle((int)globalPosition.X, (int)globalPosition.Y + 24, tileSize, 8));
        _staticHitbox.BlocksLayers = HitboxLayer.All;
        _hitboxManager.AddStatic(_staticHitbox);

        _animatedSprite = new AnimatedSprite(_spriteSheet, _tileSize, _tileSize, false, false);
        _animatedSprite.BaseDrawLayer = (float)(0.8f / 1000f) * (globalPosition.Y + 32) - 0.0001f;
        this.AddChild(_animatedSprite);
    }
    public void Open()
    {
        _animatedSprite?.Play();
    }
    public void Reset()
    {
        _animatedSprite?.Reset();
    }
}