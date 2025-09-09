
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
        if (parent.instantiatedParticles is null)
        {
            parent.instantiatedParticles = new();
        }
        else if (parent.instantiatedParticles.ContainsKey(this))
        {
            return;
        }
        GpuParticles2D newParticles = particles.Instantiate<GpuParticles2D>();
        parent.instantiatedParticles[this] = newParticles;
        if (parent is not Bullet)
        {
            ((Node2D)parent).AddChild(newParticles);
        }
    }

    public override void OngoingEffect(double delta, IAffectedByVisualEffects parent)
    {

        return;
    }

}