

using Godot;

public class SmartBullet : Mutation
{

    public override string GetName()
    {
        return "Smart Bullet";
    }
    public override void ImmediateEffect(Bullet projectile)
    {

    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        Vector2 towardsMouse = projectile.GetGlobalMousePosition() - projectile.GlobalPosition;
        if (towardsMouse.LengthSquared() > 700 * 700)
        {
            return;
        }
        projectile.SetVelocity(projectile.GetCurrentVelocity() + towardsMouse.Normalized() * (float)delta * 5000);
    }


}