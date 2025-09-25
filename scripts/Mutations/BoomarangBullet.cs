using Godot;

public class BoomarangBullet : Mutation
{
    Node2D player;
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
        return "Boomarang Bullet";
    }

    public override void ImmediateEffect(Bullet projectile)
    {

    }


    public override void OngoingEffect(double delta, Bullet projectile)
    {
        float towardsPlayerRot = projectile.direction.AngleTo(State.player.GlobalPosition - projectile.position);
        Vector2 newDir = projectile.direction.Rotated(towardsPlayerRot * (float)delta * 3f);
        projectile.direction = newDir;
    }
}