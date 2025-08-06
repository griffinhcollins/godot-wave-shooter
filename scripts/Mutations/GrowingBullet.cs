

public class GrowingBullet : Mutation
{

    public override string GetName()
    {
        return "Growing Bullet";
    }
    public override void ImmediateEffect(Bullet projectile)
    {
        return;
    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        
        projectile.SetScale(projectile.GetTimeAlive() * 2 + 0.1f);
    }
    
}