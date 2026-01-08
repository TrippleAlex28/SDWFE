using System;
using System.Collections.Generic;
using Engine;
using Engine.Hitbox;
using Microsoft.Xna.Framework;

public class Stair : GameObject
{
    public List<Rectangle> GroundColliders = new();
    public List<Rectangle> PlatformColliders = new();
    public List<Rectangle> RailingColliders = new();

    public Rectangle GroundStepCollider;
    public Rectangle TopStepCollider;

    private Vector2 _stairDirection;
    private int COLLIDEROFFSET = 4;

    /// <summary>
    /// Creates a stair object with colliders for ground and when walking up from below.
    /// </summary>
    /// <param name="stepWidth">The width of each step in the stair in pixels</param>
    /// <param name="stepHeight">The height of each step in the stair in pixels</param>
    /// <param name="numberOfSteps">The total number of steps in the stair</param>
    /// <param name="direction">The direction the stair is facing (0 for right, 1 for left)</param>
    /// <param name="globalPosition">The global position of the stair in the game world</param>
    public Stair(Vector2 globalPosition, int direction = 0, int stepWidth = 8, int stepLength = 48, int stepHeight = 4, int numberOfSteps = 8)
    {
        // Calculate stair direction based on step dimensions and direction
        // Direction 0 = going right/up, 1 = going left/up
        float horizontalComponent = 64;
        float verticalComponent = -2; // negative because Y goes down in screen space
        
        _stairDirection = new Vector2(horizontalComponent, verticalComponent);
        _stairDirection.Normalize();
        
        this.GlobalPosition = globalPosition;

        for (int i = 0; i < numberOfSteps; i++)
        {
            // Rectangle railingColliderBottom = new Rectangle(
            //     (int)globalPosition.X + i * stepWidth,
            //     (int)globalPosition.Y - (i + 1) * stepHeight,
            //     stepWidth,
            //     5
            // );

            // Rectangle railingColliderTop = new Rectangle(
            //     (int)globalPosition.X + i * stepWidth,
            //     (int)globalPosition.Y - stepLength - (i + 1) * stepHeight,
            //     stepWidth,
            //     5
            // );
            // RailingColliders.Add(railingColliderBottom);
            // RailingColliders.Add(railingColliderTop);

            Rectangle stepCollider = new Rectangle(
                (int)globalPosition.X,
                (int)globalPosition.Y - stepLength - (numberOfSteps - i - 1) * stepHeight,
                stepWidth,
                stepHeight
            );
            GroundColliders.Add(stepCollider);

            Rectangle platformCollider = new Rectangle(
                (int)globalPosition.X,
                (int)globalPosition.Y + (numberOfSteps - i - 1) * stepHeight - 4,
                stepWidth,
                4
            );
            PlatformColliders.Add(platformCollider);
        }

        GroundStepCollider = new Rectangle(
            (int)globalPosition.X,
            (int)globalPosition.Y - stepLength - stepHeight + COLLIDEROFFSET,
            stepWidth,
            stepLength - COLLIDEROFFSET
        );
        TopStepCollider = new Rectangle(
            (int)globalPosition.X + (numberOfSteps - 1) * stepWidth,
            (int)globalPosition.Y - stepLength - numberOfSteps * stepHeight,
            stepWidth,
            stepLength - COLLIDEROFFSET
        );
        
    }
    public void SetStairTriggers(HitboxManager hitboxManager)
    {
        var stairTrigger = hitboxManager.AddTrigger(this.GroundStepCollider, detectsLayers: HitboxLayer.All);

        stairTrigger.OnEnter += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                entity.IsOnStairs = true;
                entity.StairDirection = _stairDirection;
                Console.WriteLine("Player entered stair ground step collider");
            }
        };

        stairTrigger.OnExit += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                if ((side & TriggerSide.Left) != 0){
                    entity.IsOnStairs = false;
                    entity.StairDirection = Vector2.Zero;
                    Console.WriteLine("Player exited stairs through the left!");
                }  
            }
        };

        var stairTopTrigger = hitboxManager.AddTrigger(this.TopStepCollider, detectsLayers: HitboxLayer.All);

        stairTopTrigger.OnEnter += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                entity.IsOnStairs = true;
                entity.StairDirection = _stairDirection;
                Console.WriteLine("Player entered stair top step collider");
            }
        };

        stairTopTrigger.OnExit += (trigger, obj, side) => {
            if (obj is GameObject entity)
            {
                if ((side & TriggerSide.Right) != 0){
                    entity.IsOnStairs = false;
                    entity.StairDirection = Vector2.Zero;
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
    /// Gets the normalized direction vector of the stairs (direction to climb)
    /// </summary>
    public Vector2 Direction => _stairDirection;

    public void CheckIfPlayerColliderIntersectsRailing(Rectangle playerHitbox)
    {
        foreach (Rectangle railing in RailingColliders)
        {
            if (playerHitbox.Intersects(railing))
            {
                Console.WriteLine("Player hitbox intersects railing at {0}", railing);
            }
        }
    }
}