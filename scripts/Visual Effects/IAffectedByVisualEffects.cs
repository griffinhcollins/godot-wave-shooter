using System.Collections.Generic;
using Godot;

public interface IAffectedByVisualEffects
{
    List<VisualEffect> visualEffects { get; set; }

    public void AddVisualEffect(VisualEffect effect)
    {
        if (visualEffects is null)
        {
            visualEffects = new List<VisualEffect>();
        }
        visualEffects.Add(effect);
        effect.ImmediateEffect((Node2D)this);
    }

    public void ImmediateVisualEffects()
    {
        if (visualEffects is null)
        {
            return;
        }
        foreach (VisualEffect e in visualEffects)
        {
            e.ImmediateEffect((Node2D)this);
        }
    }

    public void ProcessVisualEffects(float delta)
    {
        if (visualEffects is null)
        {
            return;
        }
        foreach (VisualEffect e in visualEffects)
        {
            e.OngoingEffect(delta, (Node2D)this);

        }
    }
}