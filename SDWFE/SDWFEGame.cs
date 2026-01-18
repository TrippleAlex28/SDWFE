using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Engine;
using Engine.Input;
using Engine.Network.Shared.Command;
using Engine.Network.Shared.Object;
using Engine.Network.Shared.Session;
using Engine.Scene;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SDWFE.Commands;
using SDWFE.Managers;
using SDWFE.Objects;
using SDWFE.Objects.Entities.Enemies;
using SDWFE.Objects.Entities.PlayerEntity;
using SDWFE.Objects.Inventory.Ability;
using SDWFE.Objects.Inventory.Item;
using SDWFE.Objects.Projectiles.Bullets;
using SDWFE.Scenes;
using SDWFE.Scenes.Levels;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace SDWFE;

public class SDWFEGame : ExtendedGame
{
    public static SDWFEGame Instance;
    
    public SDWFEGame()
    {
        Instance = this;
        GAME_NAME = "Song Dynasty Warrior of the Fallen Empire";
        
        // --- NET OBJECTS SETUP ---
        NetObjectRegistry.Register<Player>((uint)NetObjects.Player);
        NetObjectRegistry.Register<GenericBullet>((uint)NetObjects.GenericBullet);
        NetObjectRegistry.Register<ShotgunBullet>((uint)NetObjects.ShotgunBullet);
        NetObjectRegistry.Register<FireworkRocket>((uint)NetObjects.FireworkRocket);
        NetObjectRegistry.Register<Arrow>((uint)NetObjects.Arrow);
        NetObjectRegistry.Register<Orb>((uint)NetObjects.Orb);
        
        NetObjectRegistry.Register<Grunt>((uint)NetObjects.Grunt);
        NetObjectRegistry.Register<Turret>((uint)NetObjects.Turret);
        NetObjectRegistry.Register<Boss>((uint)NetObjects.Boss);
        
        // --- NET COMMANDS SETUP ---
        NetCommandRegistry.Register<WalkCommand>((uint)NetCommands.Move);
        NetCommandRegistry.Register<LeapCommand>((uint)NetCommands.Leap);
        NetCommandRegistry.Register<UseCommand>((uint)NetCommands.Use);
        
        // --- SCENES SETUP ---
        SceneRegistry.Register<MainMenuScene>(MainMenuScene.KEY);
        SceneRegistry.Register<HubLevel>(HubLevel.KEY);
        SceneRegistry.Register<Level1>(Level1.KEY);
        SceneRegistry.Register<Level2>(Level2.KEY);
        SceneRegistry.Register<Level3>(Level3.KEY);
        SceneRegistry.Register<Level4>(Level4.KEY);
        SceneRegistry.Register<Level5>(Level5.KEY);
        SceneRegistry.Register<Level6>(Level6.KEY);
    }

    protected override void Initialize()
    {
        base.Initialize();
        
        // Initialize Input System
        InputSetup.Initialize(InputSetup.PROFILE_UI);
        
        // Bind to GameState events
        GameState.Instance.OnDisconnected += async (reason) =>
        {
            Console.WriteLine($"Disconnect: {reason}");
            GameState.Instance.SwitchSessionAndScene(SessionType.Singleplayer, MainMenuScene.KEY);
        };
        
        // Initialize Scene & Session
        GameState.Instance.SwitchScene(MainMenuScene.KEY);
        GameState.Instance.SwitchSession(SessionType.Singleplayer);
        
        this.IsMouseVisible = true;
    }

    protected override void LoadContent()
    {
        base.LoadContent();
        
        Resources.LoadContent();
        
        // Load saved inputs
        InputManager.LoadFromFile(InputManager.GetDefaultConfigPath(GAME_NAME));
        
        // Load items
        ItemSetup.Initialize();
        ItemDatabase.Instance.LoadDatabase();
        
        // Load abilities
        AbilityRegistry.Initialize();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update the GameState
        GameState.Instance.HandleInput(CreateNetCommands());
        GameState.Instance.AdvanceClientTick();
        GameState.Instance.Update(gameTime);
        
        FollowCamera.Update(gameTime);
        
        // Attach camera to player
        if (GameState.Instance.CurrentScene != null && GameState.Instance.SessionManager.CurrentSession != null)
        {
            Vector2 followPosition;
            var pawn = GameState.Instance.CurrentScene!.GetPawn(GameState.Instance.SessionManager.CurrentSession!
                .LocalClientId);
            if (pawn != null)
                followPosition = pawn.GlobalPosition + pawn.CameraOffset;
            else
                followPosition = Vector2.Zero;
            
            FollowCamera.Follow(followPosition);
        }
    }

    private bool _createdWalkCommand = false;
    private List<NetCommand> CreateNetCommands()
    {
        var commands = new List<NetCommand>();

        var scene = GameState.Instance.CurrentScene!;
        var pawn = scene.GetPawn(GameState.Instance.SessionManager.CurrentSession!
            .LocalClientId);
        if (pawn is Player player)
        {
            var lookDirection = (ScreenToWorld(InputManager.Instance.MousePosition.ToVector2()) -
                                 (player.GlobalPosition + player.CameraOffset));
            
            #region Movement
            
            // ===== WALKING =====
            bool createWalkCommand = false;
            var walkDirection = new Vector2();
            if (InputManager.Instance.IsActionDown(InputSetup.ACTION_MOVE_UP))
            {
                createWalkCommand = true;
                walkDirection.Y -= 1;
            }
            if (InputManager.Instance.IsActionDown(InputSetup.ACTION_MOVE_DOWN))
            {
                createWalkCommand = true;
                walkDirection.Y += 1;
            }
            if (InputManager.Instance.IsActionDown(InputSetup.ACTION_MOVE_LEFT))
            {
                createWalkCommand = true;
                walkDirection.X -= 1;
            }
            if (InputManager.Instance.IsActionDown(InputSetup.ACTION_MOVE_RIGHT))
            {
                createWalkCommand = true;
                walkDirection.X += 1;
            }
            
            if (createWalkCommand && player.CanWalk)
            {
                _createdWalkCommand = true;
                commands.Add(new WalkCommand(walkDirection));
            }
            else if (_createdWalkCommand)
            {
                _createdWalkCommand = false;
                commands.Add(new WalkCommand(Vector2.Zero));
            }
            
            // ===== LEAPING =====
            bool createLeapCommand = false;
            var leapDirection = new Vector2();
            if (InputManager.Instance.IsActionDown(InputSetup.ACTION_LEAP))
            {
                createLeapCommand = true;
                if (walkDirection == Vector2.Zero)
                {
                    leapDirection = lookDirection;
                }
                else
                {
                    leapDirection = walkDirection;
                }
            }
            if (createLeapCommand && player.CanLeap) commands.Add(new LeapCommand(leapDirection));
            
            #endregion
            
            #region Inventory

            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_USE))
            {
                if (player.Inventory.GetSelectedItem() != null && player.Inventory.GetSelectedItem()!.Data.UseActionId != null && player.Inventory.GetSelectedItem()!.StackSize > 0)
                {
                    if (player.Inventory.GetSelectedItem()!.Data.ItemType != ItemType.Weapon)
                    {
                        player.Inventory.GetSelectedItem()!.RemoveStack();
                        player.Inventory.ForceRefresh();
                    }
                    commands.Add(new UseCommand(player.Inventory.GetSelectedItem()!.Name, lookDirection));
                }
            }

            if (InputManager.Instance.IsActionPressed(InputSetup.ACTION_SHOOT))
            {
                if (player.Inventory.GetEquippedWeapon() != null && player.Inventory.GetEquippedWeapon()!.WeaponData.UseActionId != null)
                {
                    commands.Add(new UseCommand(player.Inventory.GetEquippedWeapon()!.Name, lookDirection));
                }
            }

            #endregion
        }
        
        return commands;
    }
}