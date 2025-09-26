using Godot;

public class BoomarangBullet : Mutation
{
    Node2D player;
    float baseDamage;
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
        baseDamage = projectile.GetDamage();
    }


    public override void OngoingEffect(double delta, Bullet projectile)
    {
        float boomarangRot;
        if (projectile.seed > 0.5f)
        {
            boomarangRot = projectile.direction.AngleTo(-projectile.initialDirection) + 1f;
        }
        else
        {
            boomarangRot = (-projectile.initialDirection).AngleTo(projectile.direction) + 1f;
        }
        float boomarangSpeed = Mathf.Max(Mathf.Abs(projectile.direction.Dot(-projectile.initialDirection)), 0.7f);
        Vector2 newDir = projectile.direction.Rotated(boomarangRot * (float)delta * 3f * (projectile.seed > 0.5f ? 1 : -1));
        projectile.direction = newDir;
        projectile.speed = boomarangSpeed * projectile.initialSpeed;
        projectile.SetBaseDamage(baseDamage * (Mathf.Max(0.5f, projectile.lifetime) * 2f));
    }
}