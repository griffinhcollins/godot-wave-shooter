

using Godot;

public class ChaoticBullet : Mutation
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
        return "Chaotic Bullet";
    }
    public override void ImmediateEffect(Bullet projectile)
    {

    }

    public override void OngoingEffect(double delta, Bullet projectile)
    {

        Vector2 newDir = projectile.direction.Rotated((GD.Randf() > 0.5f ? 1.1f : -1.1f) * Mathf.Pi / 2);
        projectile.direction = projectile.direction + newDir * (float)delta * 10f;
    }


}