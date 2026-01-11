using System;
using System.Runtime.Intrinsics.X86;
using Engine;
using Engine.Input;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    public const float WALK_VELOCITY = 100f;

    public bool CanWalk => !_isLeaping;

    // Leap Properties
    public const float LEAP_VELOCITY = 800;
    public const float LEAP_DURATION = 0.1f;
    public const float LEAP_COOLDOWN = 2.0f;
    
    // Leap Management
    public bool CanLeap => !_isLeaping;
    private bool _isLeaping = false;
    private float _currentLeapTime = 0f;
    private float _currentLeapCooldown = 0f;
    private Vector2 _currentLeapVelocity = Vector2.Zero;

    public void Leap(Vector2 direction)
    {
        if (!CanLeap) return;
        if (Stats.CurrentStamina < 50f) return;
        if (direction.IsApproximatelyZero()) return;
        if (_currentLeapCooldown > 0f) return;

        Stats.CurrentStamina -= 50f;
        _isLeaping = true;
        _currentLeapCooldown = LEAP_COOLDOWN;
        _currentLeapTime = LEAP_DURATION;
        _currentLeapVelocity = direction * LEAP_VELOCITY;
    }

    private void UpdateLeap(GameTime gameTime)
    {
        float dt = gameTime.DeltaSeconds();

        _currentLeapCooldown -= dt;
        _currentLeapTime -= dt;

        if (_currentLeapTime <= 0f)
        {
            _isLeaping = false;
            _currentLeapVelocity = Vector2.Zero;
        }
        else
        {
            // Apply leap movement
            this.GlobalPosition += _currentLeapVelocity * dt;
        }
    }

    private void UpdateMovement(GameTime gameTime)
    {
        if (CanWalk)
            this.Velocity = WALK_VELOCITY;
        else
            this.Velocity = 0;

        UpdateLeap(gameTime);
    }
}