using Engine;
using Engine.Hitbox;
using Engine.Network.Shared.Session;
using Microsoft.Xna.Framework;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Scenes.Levels;

public class Level6 : GameplayLevel
{
    public const string KEY = "Level6";

    public TriggerHitbox DialogueTriggerHitbox;
    public Level6() : base(KEY, KEY)
    {
        
    }

    public override void Enter()
    {
        LevelIndex = 6;
        base.Enter();

        Boss boss = waveManager.bossEntity;
        bool idle = boss.IsIdle;
        DialogueTriggerHitbox = new TriggerHitbox(new Rectangle(
            (int)(boss.GlobalPosition.X - 64),
            (int)(boss.GlobalPosition.Y - 64),
            128,
            128))
        {
            DetectsLayers = HitboxLayer.Player
        };
        HitboxManager.AddTrigger(DialogueTriggerHitbox);
        DialogueTriggerHitbox.OnEnter += (hitbox, other, side) =>
        {
            StartDialogueBoss();
        };
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
    }

    public void StartDialogueBoss()
    {
        
        foreach (var playerObject in GetAllPawns())
        {
            if (playerObject is Player player)
            {
                player.ShowChoiceDialogue("boss_intro");
                if (GameState.Instance.SessionManager.IsHost || !GameState.Instance.SessionManager.IsMultiplayer)
                {
                    player.DialogueChoice.OnDialogueClosed += () =>
                    {
                        Boss boss = waveManager.bossEntity;
                        boss.IsIdle = false;
                        HitboxManager.RemoveTrigger(DialogueTriggerHitbox);
                    };
                }
            }
        }
    }
}