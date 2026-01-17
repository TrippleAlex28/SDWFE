using Engine.Hitbox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDWFE.Objects.Entities.PlayerEntity;

public class ShopKeeper : NPC
{
    public ShopKeeper(Rectangle interactionBox, Texture2D idle_spritesheet, HitboxManager hitboxManager)
        : base("wizard_root", interactionBox, idle_spritesheet, hitboxManager, 16)
    {
        
    }

    public override void StartDialogue(Player player)
    {
        if (player.IsLocallyOwned())
        {
            player.ToggleShop();
        }
    }
}