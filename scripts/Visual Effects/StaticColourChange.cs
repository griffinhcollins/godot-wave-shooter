
using System.Collections.Generic;
using Godot;

public class StaticColourChange : VisualEffect
{

    public Color modulate;
    float lifeTime;
    public float strength;
    public float priority;


    public StaticColourChange(Improvement source, Color baseColour, float _strength, float _priority, float duration = Mathf.Inf, List<Improvement> overwrites = null) : base(source, overwrites)
    {
        modulate = baseColour;
        strength = _strength;
        lifeTime = duration;
        priority = _priority;
    }


    public override string GetName()
    {
        return modulate.ToString();
    }

    public override void ImmediateEffect(IAffectedByVisualEffects parent)
    {
        applied = true;
        parent.AddStaticColour((Node2D)parent, this);
    }

    public override void OngoingEffect(double delta, IAffectedByVisualEffects parent)
    {
        lifeTime -= (float)delta;
        if (applied && lifeTime <= 0)
        {
            applied = false;
            parent.RemoveStaticColour((Node2D)parent, this);
        }
        return;
    }


}