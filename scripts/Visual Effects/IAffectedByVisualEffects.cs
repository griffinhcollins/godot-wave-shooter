using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IAffectedByVisualEffects
{
    List<VisualEffect> visualEffects { get; set; }

    List<Color> staticColours { get; set; }

    public void AddVisualEffect(VisualEffect effect)
    {
        if (visualEffects is null)
        {
            visualEffects = new List<VisualEffect>();
        }
        visualEffects.Add(effect);
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
            e.ImmediateEffect(this);
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
        Node2D sprite = parent.GetNode<Node2D>("MainSprite");
        
        if (sprite is Sprite2D)
        {
            sprite.Modulate = colour;
        }
        else if (sprite is AnimatedSprite2D)
        {
            sprite.Modulate = colour;
        }
        else
        {
            throw new System.NotImplementedException("Weird sprite type");

        }
    }
}