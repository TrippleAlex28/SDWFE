using System;
using System.Collections.Generic;
using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Tilemap;

public enum StairFacing
{
    Right = 0,
    Left = 1
}

public class Stair : GameObject
{
    #region Layer Offsets (for Y-sorting)
    
    public const float STAIR_LAYER_OFFSET = 0.004f;
    public const float BACK_RAILING_LAYER_OFFSET = 0.003f;
    public const float PLAYER_LAYER_OFFSET = 0.002f;
    public const float FRONT_RAILING_LAYER_OFFSET = 0.001f;
    
    #endregion

    #region Step Configuration
    
    private const int STEP_WIDTH = 8;
    private const int STEP_LENGTH = 48;
    private const int STEP_HEIGHT = 4;
    private const int STEP_COUNT = 8;
    private const int TILE_SIZE = 16;
    
    private const int TRIGGER_OFFSET = 8;
    private const int EXIT_SAFEGUARD_SIZE = 100;
    
    private const int TILE_WIDTH = 4;
    private const int TILE_HEIGHT = 6;
    
    #endregion

    #region Public Properties
    
    public new StairFacing Direction { get; }
    public int BottomFloor { get; set; } = 0;
    public int TopFloor { get; set; } = 1;
    
    public List<Sprite> StairSprites { get; } = new();
    public List<Sprite> BackRailingSprites { get; } = new();
    public List<Sprite> FrontRailingSprites { get; } = new();
    
    #endregion

    #region Private Fields
    
    private readonly Vector2 _movementDirection;
    
    private TriggerHitbox _bottomEnterTrigger = null!;
    private TriggerHitbox _bottomExitTrigger = null!;
    private TriggerHitbox _topEnterTrigger = null!;
    private TriggerHitbox _topExitTrigger = null!;
    
    #endregion

    #region Constructor
    
    public Stair(Vector2 position, StairFacing direction)
    {
        GlobalPosition = position;
        Direction = direction;
        _movementDirection = CalculateMovementDirection();
        
        CreateTriggers();
    }
    
    #endregion

    #region Public Methods
    
    public void RegisterHitboxes(HitboxManager hitboxManager)
    {
        hitboxManager.AddTrigger(_bottomEnterTrigger);
        hitboxManager.AddTrigger(_bottomExitTrigger);
        hitboxManager.AddTrigger(_topEnterTrigger);
        hitboxManager.AddTrigger(_topExitTrigger);
        
        SetupTriggerCallbacks();
    }

    public Rectangle GetTileRegion()
    {
        int tileX = (int)(GlobalPosition.X / TILE_SIZE);
        int tileY = (int)(GlobalPosition.Y / TILE_SIZE);

        return Direction == StairFacing.Right
            ? new Rectangle(tileX, tileY - TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT + 1)
            : new Rectangle(tileX - TILE_WIDTH, tileY - TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT + 1);
    }

    public float GetYSortBase()
    {
        return ExtendedGame.GetYSort(this.GlobalPosition, new Vector2(0, -STEP_HEIGHT)) + STAIR_LAYER_OFFSET;
    }

    public float GetPlayerYSort()
    {
        return GetYSortBase() - PLAYER_LAYER_OFFSET;
    }
    
    #endregion

    #region Trigger Setup
    
    private Vector2 CalculateMovementDirection()
    {
        float horizontal = STEP_WIDTH * STEP_COUNT;
        float vertical = -STEP_HEIGHT * STEP_COUNT;

        if (Direction == StairFacing.Left)
            horizontal = -horizontal;

        var direction = new Vector2(horizontal, vertical);
        direction.Normalize();
        return direction;
    }

    private void CreateTriggers()
    {
        CreateBottomTriggers();
        CreateTopTriggers();
    }

    private void CreateBottomTriggers()
    {
        int x = (int)GlobalPosition.X;
        int y = (int)GlobalPosition.Y - STEP_LENGTH - STEP_HEIGHT + TRIGGER_OFFSET / 2;
        int height = STEP_LENGTH - TRIGGER_OFFSET;

        if (Direction == StairFacing.Left)
            x -= STEP_WIDTH;

        var enterBounds = new Rectangle(x, y, STEP_WIDTH, height);
        _bottomEnterTrigger = new TriggerHitbox(enterBounds) { DetectsLayers = HitboxLayer.All };

        _bottomExitTrigger = new TriggerHitbox(
            x,
            y - EXIT_SAFEGUARD_SIZE / 2,
            STEP_WIDTH,
            height + EXIT_SAFEGUARD_SIZE
        );
    }

    private void CreateTopTriggers()
    {
        int x = (int)GlobalPosition.X + (STEP_COUNT - 1) * STEP_WIDTH;
        int y = (int)GlobalPosition.Y - STEP_LENGTH - STEP_COUNT * STEP_HEIGHT + TRIGGER_OFFSET / 2;
        int height = STEP_LENGTH - TRIGGER_OFFSET;

        if (Direction == StairFacing.Left)
            x = (int)GlobalPosition.X - STEP_COUNT * STEP_WIDTH;

        var enterBounds = new Rectangle(x, y, STEP_WIDTH, height);
        _topEnterTrigger = new TriggerHitbox(enterBounds) { DetectsLayers = HitboxLayer.All };

        _topExitTrigger = new TriggerHitbox(
            x,
            y - EXIT_SAFEGUARD_SIZE / 2,
            STEP_WIDTH,
            height + EXIT_SAFEGUARD_SIZE
        );
    }

    private void SetupTriggerCallbacks()
    {
        float playerYSort = GetPlayerYSort();

        // Bottom enter
        _bottomEnterTrigger.OnEnter += (trigger, obj, side) =>
        {
            if (obj is not GameObject entity || entity.ElevationLevel != BottomFloor)
                return;

            entity.IsOnStairs = true;
            entity.StairDirection = _movementDirection;
            entity.StairYSort = playerYSort;
        };

        // Bottom exit
        _bottomExitTrigger.OnExit += (trigger, obj, side) =>
        {
            if (obj is not GameObject entity || !entity.IsOnStairs)
                return;

            bool exitingBottom = (Direction == StairFacing.Right && (side & TriggerSide.Left) != 0) ||
                                 (Direction == StairFacing.Left && (side & TriggerSide.Right) != 0);

            if (exitingBottom)
            {
                entity.IsOnStairs = false;
                entity.StairDirection = Vector2.Zero;
                entity.ElevationLevel = BottomFloor;
                entity.StairYSort = 0f;
            }
        };

        // Top enter
        _topEnterTrigger.OnEnter += (trigger, obj, side) =>
        {
            if (obj is not GameObject entity || entity.ElevationLevel != TopFloor)
                return;

            entity.IsOnStairs = true;
            entity.StairDirection = _movementDirection;
            entity.StairYSort = playerYSort;
        };

        // Top exit
        _topExitTrigger.OnExit += (trigger, obj, side) =>
        {
            if (obj is not GameObject entity || !entity.IsOnStairs)
                return;

            bool exitingTop = (Direction == StairFacing.Right && (side & TriggerSide.Right) != 0) ||
                              (Direction == StairFacing.Left && (side & TriggerSide.Left) != 0);

            if (exitingTop)
            {
                entity.IsOnStairs = false;
                entity.StairDirection = Vector2.Zero;
                entity.ElevationLevel = TopFloor;
                entity.StairYSort = 0f;
            }
        };
    }
    
    #endregion
}
