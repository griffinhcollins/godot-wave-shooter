

using Godot;

public abstract class VisualEffect
{
    public bool applied = false;

    public abstract string GetName();

    public abstract void ImmediateEffect(Node2D parent);

    public abstract void OngoingEffect(double delta, Node2D parent);

    // Optional
    public virtual void OnCollision(Node2D parent)
    {
        return;
    }


}