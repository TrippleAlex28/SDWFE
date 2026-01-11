using System;
using Engine;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.Objects.Entities.Enemies;

public class Grunt : ChasingEnemy
{
    public override uint TypeId => (uint)NetObjects.Grunt;

    public Sprite Sprite { get; private set; } // TODO: Replace with Animated Sprite
    
    public Grunt() : base(100, 100f, 32f, 10f, 1.0f)
    {
        
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        
        Sprite = new Sprite(ExtendedGame.AssetManager.LoadTexture("32x32 Han_Soldier_Idle", "Entities/NPC/"));
        // Sprite = new Sprite(ExtendedGame.AssetManager.LoadTexture("Grunt", "Entities/Enemies/"));
        AddChild(Sprite);
    }

    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
        
        // TODO: Update animation stuff here
    }

    protected override void Attack()
    {
        base.Attack();

        if (Target == null) return;
        
        // Check if target is within range
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        
        // TODO: Play some effect and spawn items
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        // TODO: Possibly draw a small healthbar
    }
}
