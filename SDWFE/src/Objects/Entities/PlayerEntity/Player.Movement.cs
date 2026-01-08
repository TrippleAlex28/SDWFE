using System;
using Engine;
using Engine.Input;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    public const float _walkVelocity = 100f;

    public bool CanWalk => !_isLeaping;

    // Leap Properties
    public const float _leapVelocity = 800;
    public const float LeapDuration = 0.1f;
    
    // Leap Management
    public bool CanLeap => !_isLeaping;
    private bool _isLeaping = false;
    private float _currentLeapTime = 0f;
    private Vector2 _currentLeapVelocity = Vector2.Zero;

    public void Leap(Vector2 direction)
    {
        if (!CanLeap) return;
        if (Stats.CurrentStamina < 50f) return;
        if (direction.IsApproximatelyZero()) return;

        Stats.CurrentStamina -= 50f;
        _isLeaping = true;
        _currentLeapTime = LeapDuration;
        _currentLeapVelocity = direction * _leapVelocity;
    }

    private void UpdateLeap(GameTime gameTime)
    {
        float dt = gameTime.DeltaSeconds();
        
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
            this.Velocity = _walkVelocity;
        else
            this.Velocity = 0;

        UpdateLeap(gameTime);
    }
}