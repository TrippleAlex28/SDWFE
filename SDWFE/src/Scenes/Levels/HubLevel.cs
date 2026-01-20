using Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;

namespace SDWFE.Scenes.Levels;

public class HubLevel : GameplayLevel
{
    public const string KEY = "HubLevel";
    
    private const string INTRO_DIALOGUE = "Greatings mighty warrior, |p You were choosen by the emperor to battle the mighty wizard, who has cursed this land with a terrible spell. This curse will cause our empire to fall in an endless shadow, and we will never know peace again. You have to battle your way through the temple and fight countless enemies, and it will not be easy, but you are the only one who can do it. |p You will have to enter multiple levels of this temple, and fight your way through rooms and waves full of enemies to reach the evil wizzard at the top, who you will hopefully defeat. |p Good luck on your quest warrior, our fate depends on you.";
    
    public HubLevel() : base(KEY, "Hub")
    {
        
    }

    public override void Enter()
    {
        LevelIndex = -1;
        base.Enter();

        Vector2 shopkeeperPosition = new Vector2(30, 608 - 32);
        Texture2D shopkeeperTexture = ExtendedGame.AssetManager.LoadTexture("32x16 Idle-Sheet", "Entities/NPC/");
        ShopKeeper shopkeeper = new ShopKeeper(new Rectangle((int)shopkeeperPosition.X - 12, (int)shopkeeperPosition.Y - 12, 56, 56), shopkeeperTexture, HitboxManager);
        shopkeeper.GlobalPosition = shopkeeperPosition;
        this.AddObject(shopkeeper);
        
    }
    public override void Update(GameTime gameTime)
    {
        
        base.Update(gameTime);
        
        // Show player intro dialogue
        if (!SceneData.hasSeenIntro)
        {
            var pawn = GetPawn(GameState.Instance.SessionManager.CurrentSession?.LocalClientId ?? 0);
            if (pawn is Player player) 
                player.ShowDialogue(INTRO_DIALOGUE);
            
            SceneData.hasSeenIntro = true;
        }
    }
}