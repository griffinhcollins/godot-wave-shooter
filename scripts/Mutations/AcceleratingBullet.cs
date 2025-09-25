

using Godot;

public class AcceleratingBullet : Mutation
{


    public override bool AffectsMovement()
    {
        return true;
    }
    public override bool AffectsTexture()
    {
        return false;
    }
    public override string GetName()
    {
        return "Accelerating Bullet";
    }

    public override void ImmediateEffect(Bullet projectile)
    {
        projectile.speed = projectile.speed * 0.1f;
    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        GD.Print(projectile.speed);
        projectile.speed = projectile.speed * (1 + (float)delta * 4);
        GD.Print(projectile.speed);
    }

    public override void OnCollision(Bullet projectile)
    {
        base.OnCollision(projectile);
        ImmediateEffect(projectile);
    }
}