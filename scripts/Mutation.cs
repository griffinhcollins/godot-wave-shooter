

using Godot;

public abstract class Mutation // Comes with unlockables, not optional. Some are more useful than others.
{
    public bool applied = false;

    public abstract void ImmediateEffect(Bullet projectile);

    public abstract void OngoingEffect(double delta, Bullet projectile);


}