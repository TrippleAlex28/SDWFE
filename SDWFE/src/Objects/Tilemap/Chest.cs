using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Chest : GameObject
{
    AnimatedSprite Sprite {  get; set; }
    TriggerHitbox triggerHitbox;
    bool isTriggered = false;

    bool IsOpened = false;
    bool ShouldAnimate = false;
    public Chest(Vector2 position)
    {
        GlobalPosition = position;
        Texture2D spriteSheet = ExtendedGame.AssetManager.LoadTexture("TM_Chest", "Tilemap/");
        Sprite = new AnimatedSprite(spriteSheet, 26, 20)
        {
            SourceRectangle = new Rectangle(new Point(0, 0), new Point(26, 20)),
            OriginType = OriginType.TopLeft,
        };
        Sprite.BaseDrawLayer = 0.9f;
        this.AddChild(Sprite);
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        OpenChestAnimation(gameTime);
        if (!IsOpened && isTriggered)
        {
            IsOpened = true;
            while (ShouldAnimate == true)
            {
                OpenChestAnimation(gameTime);
            }
        }
    }

    public void RegisterHitboxes(HitboxManager hitboxManager)
    {
        triggerHitbox = new TriggerHitbox(new Rectangle(new Point((int)this.GlobalPosition.X - 10, (int)this.GlobalPosition.Y - 10), new Point(46, 40)));
        hitboxManager.AddStatic(new Rectangle(new Point((int)this.GlobalPosition.X, (int)this.GlobalPosition.Y), new Point(26, 20)));
        hitboxManager.AddTrigger(triggerHitbox);
        triggerHitbox.OnEnter += (hitbox, otherobject, side) => 
        {
            if (otherobject is GameObject obj)
            {

            }
            isTriggered = true;
        };
    }

    private float _animationTime = 0f;
    private void OpenChestAnimation(GameTime gameTime)
    {
        _animationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_animationTime <= 0.5f)
        {
            Sprite.SourceRectangle = new Rectangle(new Point(0, 0), new Point(26, 20));
        }
        else if (_animationTime <= 1f)
        {
            Sprite.SourceRectangle = new Rectangle(new Point(32, 0), new Point(26, 20));
        }
        else if (_animationTime <= 1.5f)
        {
            Sprite.SourceRectangle = new Rectangle(new Point(64, 0), new Point(26, 20));
        }
        else if (_animationTime <= 2f)
        {
            Sprite.SourceRectangle = new Rectangle(new Point(96, 0), new Point(26, 20));
        }
        else if (_animationTime >= 2.5f)
        {
            _animationTime = 0f;
            ShouldAnimate = false;
        }
    }
    
    public void GenerateReward()
    {
        Random random = new Random();
        random.Next(0,5);


    }
}