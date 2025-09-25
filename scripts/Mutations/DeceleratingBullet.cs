

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
        projectile.SetVelocity(projectile.GetInitialVelocity() * 2, false);
    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        projectile.SetVelocity(projectile.GetCurrentVelocity() * (1 - (float)delta * 4), false);
    }
    
    public override void OnCollision(Bullet projectile)
    {
        base.OnCollision(projectile);
        ImmediateEffect(projectile);
    }
}