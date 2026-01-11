using Engine.Dialogue;
using Engine;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using Engine.Sprite;
using Microsoft.Xna.Framework.Graphics;
using Engine.Hitbox;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE;

#nullable enable
public class NPC : Interactable
{
    public int NPCSIZE = 32;
    public Sprite speechBubble;
    public AnimatedSprite? idleSprite;

    public string _basenode;


    public NPC(string base_node, Rectangle interactionBox, Texture2D idleSpritesheet, HitboxManager hitboxManager) : base(interactionBox, hitboxManager)
    {
        _basenode = base_node;

        idleSprite = new AnimatedSprite(idleSpritesheet, 32, 32, true);
        AddChild(idleSprite);


        speechBubble = new Sprite(ExtendedGame.AssetManager.LoadTexture("16x32 Speechbubble_Sheet", "Entities/NPC/"))
        {
            SourceRectangle = new Rectangle(new Point(0, 0), new Point(16, 16)),
            LocalPosition = new Vector2(2, -5)
        };
        speechBubble.IsVisible = false;
        this.AddChild(speechBubble);
    }

    public void ToggleSpeechBubble()
    {   
        speechBubble.IsVisible = !speechBubble.IsVisible;
    }
    public void StartDialogue(Player player)
    {
        if (player.IsLocallyOwned() && player.DialogueChoice != null)
        {
            player.DialogueChoice.Show(_basenode);
        }
    }


    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);

    }
}