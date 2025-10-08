

using Godot;

public class GrowingBullet : Mutation
{
    public override bool AffectsMovement()
    {
        return false;
    }
    public override bool AffectsTexture()
    {
        return true;
    }
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
        projectile.SetScale(projectile.GetTimeAlive() * 2 + 0.5f);
    }

}