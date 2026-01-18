using Engine;
using Microsoft.Xna.Framework;

namespace SDWFE.Objects.Entities.PlayerEntity;

public partial class Player
{
    private float shootCooldown = 0f;

    public bool CanShoot => shootCooldown <= 0f;
    
    public void SetShootCooldown(float cd) => shootCooldown = cd;
    public void SetShootCooldownFromAttackSpeed(float attackSpeed) => shootCooldown = 1f / attackSpeed;
    
    private void ResetShootCooldown() => shootCooldown = 0f;
    private void UpdateWeapons(GameTime gameTime)
    {
        shootCooldown -= gameTime.DeltaSeconds();
    }
}