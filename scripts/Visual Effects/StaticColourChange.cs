
using System.Collections.Generic;
using Godot;

public class StaticColourChange : VisualEffect
{

    Color modulate;
    float lifeTime;


    public StaticColourChange(Improvement source, Color baseColour, float duration = Mathf.Inf, List<Improvement> overwrites = null) : base(source, overwrites)
    {
        modulate = baseColour;
        lifeTime = duration;
    }


    public override string GetName()
    {
        return modulate.ToString();
    }

    public override void ImmediateEffect(IAffectedByVisualEffects parent)
    {
        applied = true;
        parent.AddStaticColour(modulate);
    }

    public override void OngoingEffect(double delta, IAffectedByVisualEffects parent)
    {
        lifeTime -= (float)delta;
        if (applied && lifeTime <= 0)
        {
            applied = false;
            parent.RemoveStaticColour(modulate);
        }
        return;
    }


}