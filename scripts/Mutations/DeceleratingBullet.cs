

using Godot;

public class DeceleratingBullet : Mutation
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
        return "Decelerating Bullet";
    }
    public override void ImmediateEffect(Bullet projectile)
    {
        projectile.speed = projectile.initialSpeed * 2;
        if (!projectile.isShard) { projectile.lifetime = -5f; }
    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        projectile.speed = projectile.speed * (1 - (float)delta * 6);
    }

    public override void OnCollision(Bullet projectile)
    {
        base.OnCollision(projectile);
        projectile.direction = -projectile.direction;
        ImmediateEffect(projectile);
    }
}