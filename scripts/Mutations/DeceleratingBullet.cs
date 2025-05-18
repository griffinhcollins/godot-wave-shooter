

public class DeceleratingBullet : Mutation
{
    public override void ImmediateEffect(Bullet projectile)
    {
        projectile.SetVelocity(projectile.GetCurrentVelocity() * 2, false);
    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        projectile.SetVelocity(projectile.GetCurrentVelocity() * (1 - (float)delta*4), false);
    }
}