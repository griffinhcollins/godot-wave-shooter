
using Godot;

public class ParticleEffect : VisualEffect
{

    string name;
    PackedScene particles; // Used for temporary effects

    public ParticleEffect(string path)
    {
        name = path;
        particles = ResourceLoader.Load<PackedScene>("res://player/effects/upgrade particles/" + path);

    }


    public override string GetName()
    {
        return name;
    }

    public override void ImmediateEffect(Node2D parent)
    {
        parent.AddChild(particles.Instantiate<GpuParticles2D>());
    }

    public override void OngoingEffect(double delta, Node2D parent)
    {
       
        return;
    }

}