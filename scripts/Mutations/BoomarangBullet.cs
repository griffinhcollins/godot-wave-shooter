using Godot;

public class BoomarangBullet : Mutation
{
    Node2D player;
    public override string GetName()
    {
        return "Boomarang Bullet";
    }

    public override void ImmediateEffect(Bullet projectile)
    {

    }


    public override void OngoingEffect(double delta, Bullet projectile)
    {
        Vector2 towardsPlayer = State.player.GlobalPosition - projectile.position;
        projectile.SetVelocity(projectile.GetCurrentVelocity() + towardsPlayer * (float)delta * 50);
    }
}