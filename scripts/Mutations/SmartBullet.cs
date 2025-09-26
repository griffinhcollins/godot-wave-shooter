

using Godot;

public class SmartBullet : Mutation
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
        return "Smart Bullet";
    }
    public override void ImmediateEffect(Bullet projectile)
    {

    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        Vector2 towardsMouse = State.bulletManager.GetGlobalMousePosition() - projectile.position;
        if (towardsMouse.LengthSquared() > 700 * 700)
        {
            return;
        }
        float angleToMouse = projectile.direction.AngleTo(towardsMouse);
        projectile.direction = projectile.direction.Rotated(angleToMouse * (float)delta * 5f);
    }


}