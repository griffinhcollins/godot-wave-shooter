
using System.Collections.Generic;
using Godot;

public class ParticleEffect : VisualEffect
{

    string name;
    PackedScene particles;

    public ParticleEffect(Improvement source, string name, List<Improvement> overwrites = null) : base(source, overwrites)
    {
        this.name = name;
        particles = ResourceLoader.Load<PackedScene>("res://player/effects/upgrade particles/" + name.ToLower() + "_particles.tscn");

    }


    public override string GetName()
    {
        return name;
    }

    public override void ImmediateEffect(IAffectedByVisualEffects parent)
    {
        if (!applied)
        {
            return;
        }
        ((Node2D)parent).AddChild(particles.Instantiate<GpuParticles2D>());
    }

    public override void OngoingEffect(double delta, IAffectedByVisualEffects parent)
    {

        return;
    }

}