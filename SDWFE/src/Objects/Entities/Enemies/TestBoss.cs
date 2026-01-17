//// DELETE AFTER DEMO


using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Entities.PlayerEntity;

public class TestBoss : Enemy
{
    public override uint TypeId => (uint)NetObjects.Grunt;

    public AnimatedSprite Sprite { get; private set; } // TODO: Replace with Animated Sprite
    
    public TriggerHitbox? Hitbox { get; private set; }
    private HitboxManager _hitboxManager;
    public int Offset = 64;

    public TestBoss(HitboxManager hitboxManager) : base(100, 100f, 32f, 10f)
    {
        _hitboxManager = hitboxManager;
    }

    protected override void EnterSelf()
    {
        base.EnterSelf();
        TriggerHitbox hitbox = new TriggerHitbox(
            new Rectangle(
                (int)this.GlobalPosition.X - Offset, 
                (int)this.GlobalPosition.Y - Offset, 
                32 + Offset * 2, 
                32 + Offset * 2
            )
        );
        _hitboxManager.AddTrigger(hitbox);
        hitbox.DetectsLayers = HitboxLayer.Player;
        hitbox.OnEnter += OnPlayerEnter;
        Texture2D texture = ExtendedGame.AssetManager.LoadTexture("32x32 Han_Soldier_Idle", "Entities/NPC/");
        Sprite = new AnimatedSprite(texture, 32, 32, 200f, true, true);

        Sprite.BaseDrawLayer = ExtendedGame.GetYSort(GlobalPosition, new Vector2(0, 16));
        // Sprite = new Sprite(ExtendedGame.AssetManager.LoadTexture("Grunt", "Entities/Enemies/"));
        AddChild(Sprite);
    }
    private void OnPlayerEnter(TriggerHitbox hitbox, object other, TriggerSide side)
    {
        if (other is Player player)
        {
            player.ShowChoiceDialogue("test_boss_intro");
        }
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