

public class AcceleratingBullet : Mutation
{
    public override void ImmediateEffect(Bullet projectile)
    {
        projectile.SetVelocity(projectile.GetCurrentVelocity() * 0.2f, false);
    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        projectile.SetVelocity(projectile.GetCurrentVelocity() * (1 + (float)delta*4), false);
    }
}