

using Godot;

public abstract class VisualEffect
{
    public bool applied = false;

    public abstract string GetName();

    public abstract void ImmediateEffect(IAffectedByVisualEffects parent);

    public abstract void OngoingEffect(double delta, IAffectedByVisualEffects parent);

    // Optional
    public virtual void OnCollision(IAffectedByVisualEffects parent)
    {
        return;
    }


}