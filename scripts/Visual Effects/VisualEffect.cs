

using System.Collections.Generic;
using Godot;

public abstract class VisualEffect
{
    public bool applied = true; // Will be disabled if this effect is, for example, overwritten
    public Improvement source; // Where this visual effect came from
    public List<Improvement> overwrites; // Which sources this visual effect should overwrite (e.g. Plague overwrites Venom)


    public VisualEffect(Improvement source, List<Improvement> overwrites = null)
    {
        this.source = source;
        if (overwrites is null)
        {
            this.overwrites = new();
        }
        else
        {
            this.overwrites = overwrites;
        }
    }

    public abstract string GetName();

    public abstract void ImmediateEffect(IAffectedByVisualEffects parent);

    public abstract void OngoingEffect(double delta, IAffectedByVisualEffects parent);

    // Optional
    public virtual void OnCollision(IAffectedByVisualEffects parent)
    {
        return;
    }




}