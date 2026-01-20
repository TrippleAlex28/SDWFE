using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using Engine;
using Engine.Input;
using Engine.Sprite;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    public const float WALK_VELOCITY = 200f;

    public bool CanWalk => !_isLeaping;

    // Movement multiplier
    private float _movementMultiplier = 1f;
    private float _movementMultiplierTimer = 0f;
    
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

    // Animation State Machine
    private PlayerAnimationState _animationState = PlayerAnimationState.IdleDown;
    private PlayerAnimationState _lastAnimationState = PlayerAnimationState.None;
    private Dictionary<PlayerAnimationState, AnimationData> _animations = new();
    private bool _facingUp = false; // Track facing direction

    /// <summary>
    /// Initialize the animation system with all animation data
    /// </summary>
    private void InitializeAnimations()
    {
        var idleSheet = ExtendedGame.AssetManager.LoadTexture("32x16 Idle-Sheet", "Entities/Player/");
        var useSheet = ExtendedGame.AssetManager.LoadTexture("32x16 Use-Sheet", "Entities/Player/");
        
        _animations[PlayerAnimationState.IdleUp] = new AnimationData(idleSheet, 1, 200f, true);
        _animations[PlayerAnimationState.IdleDown] = new AnimationData(idleSheet, 0, 200f, true);
        _animations[PlayerAnimationState.WalkingUp] = new AnimationData(_runSheet, 1, 100f, true);
        _animations[PlayerAnimationState.WalkingDown] = new AnimationData(_runSheet, 0, 100f, true);
        _animations[PlayerAnimationState.HurtDown] = new AnimationData(_hurtTexture, 0, 200f, false);
        _animations[PlayerAnimationState.HurtUp] = new AnimationData(_hurtTexture, 1, 200f, false);
        _animations[PlayerAnimationState.UseUp] = new AnimationData(useSheet, 1, 80f, false);
        _animations[PlayerAnimationState.UseDown] = new AnimationData(useSheet, 0, 80f, false);
        
        // Add more animations as needed
        // _animations[PlayerAnimationState.HurtUp] = new AnimationData(hurtSheet, 0, 100f, false);
        // _animations[PlayerAnimationState.HurtDown] = new AnimationData(hurtSheet, 1, 100f, false);
        // _animations[PlayerAnimationState.Use] = new AnimationData(useSheet, 0, 80f, false);
        // _animations[PlayerAnimationState.Leaping] = new AnimationData(leapSheet, 0, 50f, false);
    }

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
        
        // Play leap animation if available
        PlayOneShotAnimation(PlayerAnimationState.Leaping);
    }

    // TODO: allows bugs but whatever
    public void ApplyMovementMultiplier(float multiplier, float time)
    {
        _movementMultiplierTimer += time;
        _movementMultiplier += multiplier;
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
        if (_movementMultiplierTimer > 0f)
            _movementMultiplierTimer -= gameTime.DeltaSeconds();

        if (_movementMultiplierTimer <= 0f && !_movementMultiplier.IsApproximatelyEqual(1f))
        {
            _movementMultiplier = 1f;
            _movementMultiplierTimer = 0f;
        }
        
        if (CanWalk)
            this.Velocity = WALK_VELOCITY * _movementMultiplier;
        else
            this.Velocity = 0;
        
        UpdateLeap(gameTime);
        UpdateAnimationState();
    }

    /// <summary>
    /// Automatically update animation state based on movement
    /// </summary>
    private void UpdateAnimationState()
    {
        // Don't interrupt one-shot animations
        if (Sprite.IsPlayingOneShot) return;
        
        // Update facing direction based on movement
        var input = InputManager.Instance;
        bool moveUp = input.IsActionDown(InputSetup.ACTION_MOVE_UP);
        bool moveDown = input.IsActionDown(InputSetup.ACTION_MOVE_DOWN);
        
        if (moveUp && !moveDown) _facingUp = true;
        else if (moveDown && !moveUp) _facingUp = false;
        
        // Determine if walking (any movement key pressed)
        bool moveLeft = input.IsActionDown(InputSetup.ACTION_MOVE_LEFT);
        bool moveRight = input.IsActionDown(InputSetup.ACTION_MOVE_RIGHT);
        bool isWalking = moveUp || moveDown || moveLeft || moveRight;
        
        if (isWalking)
        {
            SetAnimationState(_facingUp ? PlayerAnimationState.WalkingUp : PlayerAnimationState.WalkingDown);
        }
        else
        {
            SetAnimationState(_facingUp ? PlayerAnimationState.IdleUp : PlayerAnimationState.IdleDown);
        }
    }

    /// <summary>
    /// Update the sprite based on current animation state
    /// </summary>
    private void UpdateAnimation()
    {
        if (_lastAnimationState == _animationState) return;
        
        if (_animations.TryGetValue(_animationState, out var animData))
        {
            Sprite.SetAnimation(animData);
        }
        
        _lastAnimationState = _animationState;
    }

    /// <summary>
    /// Set the animation state (for looping animations like idle/walk)
    /// </summary>
    public void SetAnimationState(PlayerAnimationState newState)
    {
        if (_animationState == newState) return;
        
        _animationState = newState;
        UpdateAnimation();
    }

    /// <summary>
    /// Play a one-shot animation (like attack, hurt, use) and return to previous state
    /// </summary>
    public void PlayOneShotAnimation(PlayerAnimationState state)
    {
        if (!_animations.TryGetValue(state, out var animData)) return;
        
        Sprite.PlayOneShot(animData);
    }

    /// <summary>
    /// Play a one-shot animation with a callback when complete
    /// </summary>
    public void PlayOneShotAnimation(PlayerAnimationState state, Action? onComplete)
    {
        if (!_animations.TryGetValue(state, out var animData)) return;
        
        if (onComplete != null)
        {
            void Handler()
            {
                onComplete();
                Sprite.AnimationCompleted -= Handler;
            }
            Sprite.AnimationCompleted += Handler;
        }
        
        Sprite.PlayOneShot(animData);
    }

    /// <summary>
    /// Force play an animation directly (bypasses state machine)
    /// </summary>
    public void ForcePlayAnimation(Texture2D texture, int column, float timePerFrame = 200f, bool isLooping = true)
    {
        Sprite.SetAnimation(texture, column, timePerFrame, isLooping);
    }
    
    public void OnStatsChanged(StatType changedStat, bool decreased)
    {
        if (changedStat == StatType.CurrentHealth && decreased)
        {
            // Play hurt animation based on facing direction
            var hurtState = _facingUp ? PlayerAnimationState.HurtUp : PlayerAnimationState.HurtDown;
            PlayOneShotAnimation(hurtState);
        }
    }

    public void OnUseItem()
    {
        // Play use animation based on facing direction
        var useState = _facingUp ? PlayerAnimationState.UseUp : PlayerAnimationState.UseDown;
        PlayOneShotAnimation(useState);
    }
    public enum PlayerAnimationState
    {
        None,
        IdleUp,
        IdleDown,
        WalkingUp,
        WalkingDown,
        HurtUp,
        HurtDown,
        UseUp,
        UseDown,
        Leaping,
        Attack,
        Death
    }
}