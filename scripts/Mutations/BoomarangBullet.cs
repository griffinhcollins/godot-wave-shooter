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

    Node2D GetPlayer(Bullet projectile)
    {
        if (player is null)
        {
            foreach (Node2D p in projectile.GetTree().GetNodesInGroup("player"))
            {
                player = p;
            }
        }
        return player;
    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {
        Vector2 towardsMouse = GetPlayer(projectile).Position - projectile.GlobalPosition;
        projectile.SetVelocity(projectile.GetCurrentVelocity() + towardsMouse * (float)delta * 50);
    }
}