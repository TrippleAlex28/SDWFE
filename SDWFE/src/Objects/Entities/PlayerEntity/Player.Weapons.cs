using Engine;
using Engine.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    private float shootCooldown = 0f;

    public bool CanShoot => shootCooldown <= 0f;

    public float DamageMultiplier { get; private set; } = 1f;
    private float _damageMultiplierTimer = 0f;
    private ParticleSystem _ragePS = new();
    private ParticleEmitter _rageEmitter;
    
    private ParticleSystem _slamPS = new();
    private ParticleEmitter _slamEmitter;
    
    public void ApplyDamageMultiplier(float multiplier, float duration)
    {
        DamageMultiplier += multiplier;
        _damageMultiplierTimer += duration;
        
        _ragePS.Restart();
    }
    
    public void SetShootCooldown(float cd) => shootCooldown = cd;
    public void SetShootCooldownFromAttackSpeed(float attackSpeed) => shootCooldown = 1f / attackSpeed;
    private void ResetShootCooldown() => shootCooldown = 0f;

    public void StartSlamEffect()
    {
        _slamPS.Restart();
    }
    
    private void UpdateWeapons(GameTime gameTime)
    {
        _rageEmitter.Position = this.GlobalPosition + this.CameraOffset;
        _slamEmitter.Position = this.GlobalPosition + this.CameraOffset;
        
        shootCooldown -= gameTime.DeltaSeconds();

        if (_damageMultiplierTimer > 0f)
            _damageMultiplierTimer -= gameTime.DeltaSeconds();
        
        if (_damageMultiplierTimer <= 0f && !DamageMultiplier.IsApproximatelyEqual(1f))
        {
            DamageMultiplier = 1f;
            _damageMultiplierTimer = 0f;
            _ragePS.Stop();
        }
        
        _ragePS.Update(gameTime.DeltaSeconds());
        _slamPS.Update(gameTime.DeltaSeconds());
    }

    private void DrawWeapons(SpriteBatch spriteBatch)
    {
        _ragePS.Draw(spriteBatch);
        _slamPS.Draw(spriteBatch);
    }
}