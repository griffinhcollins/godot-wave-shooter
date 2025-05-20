

using Godot;

public class ChaoticBullet : Mutation
{

    public override string GetName()
    {
        return "Chaotic Bullet";
    }
    public override void ImmediateEffect(Bullet projectile)
    {

    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {

        Vector2 perpendicular = projectile.GetCurrentVelocity().Rotated((GD.Randf() > 0.5f ? 1 : -1) * Mathf.Pi / 2);
        projectile.SetVelocity(projectile.GetCurrentVelocity() + perpendicular * (float)delta * 15);
    }


}