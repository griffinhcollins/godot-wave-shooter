using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IAffectedByVisualEffects
{
    public List<VisualEffect> visualEffects { get; set; }

    List<Color> staticColours { get; set; }

    public HashSet<Improvement> overwrittenSources { get; set; }

    public void AddVisualEffect(VisualEffect effect)
    {

        if (visualEffects is null)
        {
            visualEffects = new();
        }
        if (overwrittenSources is null)
        {
            overwrittenSources = new();
        }
        visualEffects.Add(effect);
        foreach (Improvement i in effect.overwrites)
        {
            overwrittenSources.Add(i);
        }

        effect.ImmediateEffect(this);

    }

    public void ImmediateVisualEffects()
    {
        if (visualEffects is null)
        {
            return;
        }
        foreach (VisualEffect e in visualEffects)
        {
            if (!e.applied) { return; }
            if (overwrittenSources.Contains(e.source))
            {
                e.applied = false;
            }
            else
            {
                e.ImmediateEffect(this);
            }
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
            if (!e.applied) { continue; }
            e.OngoingEffect(delta, this);

        }
    }

    public void AddStaticColour(Color colour)
    {
        if (staticColours is null)
        {
            staticColours = new();
        }
        staticColours.Add(colour);
        SetColour((Node2D)this, GetStaticColour(Colors.White));
    }

    public void RemoveStaticColour(Color colour)
    {
        if (staticColours is null)
        {
            return;
        }
        staticColours.Remove(colour);
        SetColour((Node2D)this, GetStaticColour(Colors.White));
    }

    public Color GetStaticColour(Color baseColour)
    {
        if (staticColours is null)
        {
            return baseColour;
        }
        return staticColours.Aggregate(baseColour, (c1, c2) => c1.Blend(c2));
    }

    private void SetColour(Node2D parent, Color colour)
    {
        GD.Print(parent.Name);
        GD.Print(colour);
        Node2D sprite = parent.GetNode<Node2D>("MainSprite");


        sprite.Modulate = colour;
    }
}