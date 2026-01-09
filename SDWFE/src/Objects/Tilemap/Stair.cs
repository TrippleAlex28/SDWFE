using System;
using System.Collections.Generic;
using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;

public class Stair : GameObject
{
    public List<Rectangle> StaticColliders = new();

    public TriggerHitbox GroundStepCollider;
    public TriggerHitbox TopStepCollider;

    private Vector2 _stairDirection;
    private int COLLIDEROFFSET = 4;
    private int STATICCOLLIDERSIZE = 4;

    // Floors
    public int _beginFloor = 0;
    public int _endFloor = 1;

    // Property
    public int _direction = 0;
    public int _stepWidth = 8;
    public int _stepLength = 48;
    public int _stepHeight = 4;
    public int _numberOfSteps = 8;

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

        GroundStepCollider = new TriggerHitbox(
            (int)globalPosition.X,
            (int)globalPosition.Y - _stepLength - _stepHeight + COLLIDEROFFSET,
            _stepWidth,
            _stepLength - COLLIDEROFFSET
        );
        GroundStepCollider.DetectsLayers = HitboxLayer.All;

        TopStepCollider = new TriggerHitbox(
            (int)globalPosition.X + (_numberOfSteps - 1) * _stepWidth,
            (int)globalPosition.Y - _stepLength - _numberOfSteps * _stepHeight,
            _stepWidth,
            _stepLength - COLLIDEROFFSET
        );
        
        TopStepCollider.DetectsLayers = HitboxLayer.All;
        
    }
    public void SetHitboxes(HitboxManager hitboxManager)
    {
        
        SetStaticColliders(hitboxManager);
        SetStairTriggers(hitboxManager);
        hitboxManager.AddTrigger(GroundStepCollider);
        hitboxManager.AddTrigger(TopStepCollider);
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

        GroundStepCollider.OnEnter += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                entity.IsOnStairs = true;
                entity.StairDirection = _stairDirection;
                entity.ElevationLevel = _beginFloor;
                TopStepCollider.IsEnabled = true;
                Console.WriteLine("Player entered stair ground step collider");
            }
        };
        
        GroundStepCollider.OnExit += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                if ((side & TriggerSide.Left) != 0){
                    entity.IsOnStairs = false;
                    entity.StairDirection = Vector2.Zero;
                    entity.ElevationLevel = _beginFloor;
                    TopStepCollider.IsEnabled = false;
                    Console.WriteLine("Player exited stairs through the left!");
                }  
            }
        };

        var topHitbox = hitboxManager.AddTrigger(TopStepCollider.Bounds, HitboxLayer.All);

        TopStepCollider.OnEnter += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                entity.IsOnStairs = true;
                entity.StairDirection = _stairDirection;
                entity.ElevationLevel = _endFloor;
                GroundStepCollider.IsEnabled = true;
                Console.WriteLine("Player entered stair top step collider");
            }
        };

        TopStepCollider.OnExit += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                if ((side & TriggerSide.Right) != 0){
                    entity.IsOnStairs = false;
                    entity.StairDirection = Vector2.Zero;
                    entity.ElevationLevel = _endFloor;
                    GroundStepCollider.IsEnabled = false;
                    Console.WriteLine("Player exited stairs through the right!");
                }  
            }
        };
    }
    private void DisableHitbox()
    {
        GroundStepCollider.IsEnabled = false;
    }
    protected override void UpdateSelf(GameTime gameTime)
    {
        base.UpdateSelf(gameTime);
    }

    /// <summary>
    /// Gets the normalized direction vector of the stairs (direction to climb)
    /// </summary>
    public Vector2 Direction => _stairDirection;

}