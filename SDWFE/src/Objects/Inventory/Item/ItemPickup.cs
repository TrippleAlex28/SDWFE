using System;
using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Objects.Inventory.Item;

public class ItemPickup : GameObject
{
    private InventoryItem _item;
    
    private Sprite _sprite;
    
    public TriggerHitbox Hitbox { get; private set; }
    private HitboxManager _hitboxManager;
    
    public ItemPickup(InventoryItem item, HitboxManager hitboxManager)
    {
        _item = item;
        _hitboxManager = hitboxManager;
        
        // LOAD ICON
        _sprite = new Sprite(_item.Icon)
        {
            OriginType = OriginType.Center,
            Scale = new Vector2(16f / _item.Icon.Width, 16f / _item.Icon.Height)
        };
        AddChild(_sprite);
        
        // SETUP COLLISION
        this.CollisionSize = new Vector2(16f, 16f);
        this.CollisionOffset = new Vector2(0, 0);
        this.HitboxManager = hitboxManager;
        this.HitboxLayer = HitboxLayer.Enemy;
        this.Hitbox = new TriggerHitbox(new Rectangle(
            (int)(this.GlobalPosition.X - CollisionSize.X / 2),
            (int)(this.GlobalPosition.Y - CollisionSize.Y / 2),
            (int)CollisionSize.X,
            (int)CollisionSize.Y))
        {
            DetectsLayers = HitboxLayer.Player
        };
        this.Hitbox.OnEnter += OnPlayerCollect;
    }

    private void OnPlayerCollect(TriggerHitbox hitbox, object other, TriggerSide side)
    {
        if (other is not Player player) return;
        
        Console.WriteLine("PICKUP");
        
        if (player.Inventory.AddItem(_item))
            this.RemoveSelf();
    }

    protected override void ExitSelf()
    {
        _hitboxManager.RemoveTrigger(Hitbox);
        base.ExitSelf();
    }
}