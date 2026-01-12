using System;

namespace SDWFE.Objects.Entities.Enemies;

public class Turret : Enemy
{
    public Turret() : base(100, 50f, 8f, .25f)
    {
        
    }

    protected override void Attack()
    {
        Console.WriteLine("Turret Attack");
    }
}