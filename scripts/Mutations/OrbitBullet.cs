

using Godot;

public class OrbitBullet : Mutation
{

    public override string GetName()
    {
        return "Orbit Bullet";
    }
    public override void ImmediateEffect(Bullet projectile)
    {

    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        bool left = projectile.seed > 0.5f;

        Vector2 perpendicular = projectile.GetCurrentVelocity().Rotated((left ? 1 : -1) * Mathf.Pi / 2);
        projectile.SetVelocity(projectile.GetCurrentVelocity() + perpendicular * (float)delta * 5);
    }


    public override void OnCollision(Bullet projectile)
    {
        base.OnCollision(projectile);
        projectile.SetVelocity(projectile.GetCurrentVelocity() * -1f);
        // if (Stats.PlayerStats.Unlocks.BouncingBullets.unlocked)
        // {
        //     projectile.SetVelocity(projectile.GetCurrentVelocity().Rotated((projectile.seed > 0.5f ? 1 : -1) * Mathf.Pi / 2));
        //     projectile.Splinter(null, 1);
        //     projectile.HandleDeath();
        // }
    }
}