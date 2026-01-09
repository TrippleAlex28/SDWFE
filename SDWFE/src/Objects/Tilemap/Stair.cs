using System;
using System.Collections.Generic;
using Engine;
using Engine.Hitbox;
using Engine.Sprite;
using Microsoft.Xna.Framework;

public class Stair : GameObject
{
    public List<Rectangle> StaticColliders = new();

    public TriggerHitbox GroundStepEnterCollider;
    public TriggerHitbox TopStepEnterCollider;
    public TriggerHitbox GroundStepExitCollider;
    public TriggerHitbox TopStepExitCollider;

    private Vector2 _stairDirection;
    private int COLLIDEROFFSET = 4;
    private int STATICCOLLIDERSIZE = 4;
    private int EXITSAFEGUARDSIZE = 100;

    // Floors
    public int _beginFloor = 0;
    public int _endFloor = 1;

    // Property
    public int _direction = 0; // 0 = right, 1 = left
    public int _stepWidth = 8;
    public int _stepLength = 48;
    public int _stepHeight = 4;
    public int _numberOfSteps = 8;
    
    // Tile region properties (in tiles, not pixels)
    public int TileWidth = 4;  // 4 tiles wide
    public int TileHeight = 6; // 5 tiles high (6 including top railing)
    public int TileSize = 16;  // 16 pixels per tile
    
    // Y-Sort layer values (relative to stair point Y)
    public const float STAIR_LAYER_OFFSET = 0.04f;       
    public const float BACK_RAILING_OFFSET = 0.03f;   
    public const float PLAYER_ON_STAIR_OFFSET = 0.02f; 
    public const float FRONT_RAILING_OFFSET = 0.01f;  
    
    // Sprites in this stair's region
    public List<Sprite> StairSprites = new();
    public List<Sprite> BackRailingSprites = new();
    public List<Sprite> FrontRailingSprites = new();

    /// <summary>
    /// Creates a stair object with colliders for ground and when walking up from below.
    /// </summary>
    /// <param name="stepWidth">The width of each step in the stair in pixels</param>
    /// <param name="stepHeight">The height of each step in the stair in pixels</param>
    /// <param name="numberOfSteps">The total number of steps in the stair</param>
    /// <param name="direction">The direction the stair is facing (0 for right, 1 for left)</param>
    /// <param name="globalPosition">The global position of the stair in the game world</param>
    public Stair(Vector2 globalPosition)
    {
        // Direction = 0 - Right
        float horizontalComponent = _stepWidth * _numberOfSteps;
        float verticalComponent = -_stepHeight * _numberOfSteps;

        // Direction = 1 - Left
        if (_direction == 1)
        {
            horizontalComponent = -horizontalComponent;
        }
        
        _stairDirection = new Vector2(horizontalComponent, verticalComponent);
        _stairDirection.Normalize();
        
        this.GlobalPosition = globalPosition;
        
        StaticColliders.Add(new Rectangle(
            (int)globalPosition.X + _stepWidth,
            (int)globalPosition.Y - _stepLength - STATICCOLLIDERSIZE,
            _stepWidth * _numberOfSteps - _stepWidth,
            STATICCOLLIDERSIZE
        ));

        StaticColliders.Add(new Rectangle(
            (int)globalPosition.X + _stepWidth,
            (int)globalPosition.Y - _stepHeight - STATICCOLLIDERSIZE,
            _stepWidth * _numberOfSteps - _stepWidth,
            STATICCOLLIDERSIZE
        ));

        InitializeTriggerHitboxes();
        
    }
    private void InitializeTriggerHitboxes()
    {
        Rectangle baseGroundColl = new Rectangle(
            (int)GlobalPosition.X,
            (int)GlobalPosition.Y - _stepLength - _stepHeight + COLLIDEROFFSET,
            _stepWidth,
            _stepLength - COLLIDEROFFSET
        );
        GroundStepEnterCollider = new TriggerHitbox(
            baseGroundColl
        );
        GroundStepEnterCollider.DetectsLayers = HitboxLayer.All;

        GroundStepExitCollider = new TriggerHitbox(
            baseGroundColl.X,
            baseGroundColl.Y - (EXITSAFEGUARDSIZE / 2),
            baseGroundColl.Width,
            baseGroundColl.Height + EXITSAFEGUARDSIZE
        );

        // Top step colliders
        Rectangle baseTopColl = new Rectangle(
            (int)GlobalPosition.X + (_numberOfSteps - 1) * _stepWidth,
            (int)GlobalPosition.Y - _stepLength - _numberOfSteps * _stepHeight,
            _stepWidth,
            _stepLength - COLLIDEROFFSET
        );
        TopStepEnterCollider = new TriggerHitbox(
            baseTopColl
        );
        TopStepEnterCollider.DetectsLayers = HitboxLayer.All;

        TopStepExitCollider = new TriggerHitbox(
            baseTopColl.X,
            baseTopColl.Y - (EXITSAFEGUARDSIZE / 2),
            baseTopColl.Width,
            baseTopColl.Height + EXITSAFEGUARDSIZE
        );
    }
    public void SetHitboxes(HitboxManager hitboxManager)
    {
        
        SetStaticColliders(hitboxManager);
        SetStairTriggers(hitboxManager);
        hitboxManager.AddTrigger(GroundStepEnterCollider);
        hitboxManager.AddTrigger(GroundStepExitCollider);
        hitboxManager.AddTrigger(TopStepEnterCollider);
        hitboxManager.AddTrigger(TopStepExitCollider);
    }
    public void SetStaticColliders(HitboxManager hitboxManager)
    {
        foreach (var collider in StaticColliders)
        {
            hitboxManager.AddStatic(collider, HitboxLayer.Environment, HitboxLayer.All);
        }
    }
    public void SetStairTriggers(HitboxManager hitboxManager)
    {
        float playerYSort = GetPlayerYSort();

        GroundStepEnterCollider.OnEnter += (trigger, obj, side) => {
            
            if (obj is GameObject entity)
            {
                if (entity.ElevationLevel != _beginFloor) return;

                entity.IsOnStairs = true;
                entity.StairDirection = _stairDirection;
                entity.ElevationLevel = _beginFloor;
                entity.StairYSort = playerYSort;
                Console.WriteLine("Player entered stair ground step collider");
            }
        };
        
        GroundStepExitCollider.OnExit += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                if (!entity.IsOnStairs) return;

                // Only exit stairs if leaving through the left side (bottom of stairs)
                if ((side & TriggerSide.Left) != 0)
                {
                    entity.IsOnStairs = false;
                    entity.StairDirection = Vector2.Zero;
                    entity.ElevationLevel = _beginFloor;
                    entity.StairYSort = 0f;
                    Console.WriteLine("Player exited stairs through the left!");
                }
            }
        };

        TopStepEnterCollider.OnEnter += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                if (entity.ElevationLevel != _endFloor) return;

                entity.IsOnStairs = true;
                entity.StairDirection = _stairDirection;
                entity.ElevationLevel = _endFloor;
                entity.StairYSort = playerYSort;
                Console.WriteLine("Player entered stair top step collider");
            }
        };

        TopStepExitCollider.OnExit += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                if (!entity.IsOnStairs) return;
                // Only exit stairs if leaving through the right side (top of stairs)
                if ((side & TriggerSide.Right) != 0)
                {
                    entity.IsOnStairs = false;
                    entity.StairDirection = Vector2.Zero;
                    entity.ElevationLevel = _endFloor;
                    entity.StairYSort = 0f;
                    Console.WriteLine("Player exited stairs through the right!");
                }
            }
        };
    }
    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
    }
    
    /// <summary>
    /// Gets the tile region bounds for this stair (in tile coordinates)
    /// </summary>
    public Rectangle GetTileRegion()
    {
        int tileX = (int)(GlobalPosition.X / TileSize);
        int tileY = (int)(GlobalPosition.Y / TileSize);
        
        // Direction 0 = right (tiles go right from point)
        // Direction 1 = left (tiles go left from point)
        if (_direction == 0)
        {
            return new Rectangle(tileX, tileY - TileHeight, TileWidth, TileHeight + 1); // +1 for top railing row
        }
        else
        {
            return new Rectangle(tileX - TileWidth + 1, tileY - TileHeight, TileWidth, TileHeight + 1);
        }
    }
    
    /// <summary>
    /// Checks if a tile grid position is within this stair's region
    /// </summary>
    public bool ContainsTile(int tileX, int tileY)
    {
        return GetTileRegion().Contains(tileX, tileY);
    }
    
    /// <summary>
    /// Gets the base Y-sort value for this stair (based on stair point position)
    /// </summary>
    public float GetStairYSortBase()
    {
        return 0.8f / 1000f * (GlobalPosition.Y - _stepHeight);
    }
    
    /// <summary>
    /// Gets the Y-sort layer for the player when on this stair
    /// </summary>
    public float GetPlayerYSort()
    {
        return GetStairYSortBase() - PLAYER_ON_STAIR_OFFSET;
    }

}