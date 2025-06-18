using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IAffectedByVisualEffects
{
    public List<VisualEffect> visualEffects { get; set; }

    Dictionary<Color, float> staticColours { get; set; }

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

    public void AddStaticColour(Color colour, float strength)
    {
        if (staticColours is null)
        {
            staticColours = new();
        }
        staticColours[colour] = strength;
        (Color c, float s) = GetStaticColour(Colors.Gray);
        SetColour((Node2D)this, c, s);
    }

    public void RemoveStaticColour(Color colour)
    {
        if (staticColours is null)
        {
            return;
        }
        staticColours.Remove(colour);
        (Color c, float s) = GetStaticColour(Colors.Gray);
        SetColour((Node2D)this, c, s);
    }

    public (Color, float) GetStaticColour(Color baseColour)
    {
        if (staticColours is null)
        {
            return (baseColour, 0);
        }
        Color aggregateColour = staticColours.Aggregate(staticColours.First().Key, (c1, c2) => c1.Lerp(c2.Key, c2.Value));
        float aggregateStrength = staticColours.Aggregate(0f, (c1, c2) => c1 + c2.Value);
        return (aggregateColour, aggregateStrength);
    }

    private void SetColour(Node2D parent, Color colour, float strength)
    {
        Node2D sprite = parent.GetNode<Node2D>("MainSprite");

        if (sprite is Sprite2D)
        {
            Sprite2D castedSprite = (Sprite2D)sprite;
            ShaderMaterial shadermat = (ShaderMaterial)castedSprite.Material;
            shadermat.SetShaderParameter("input_colour", colour);
            shadermat.SetShaderParameter("strength", strength);
        }
        else if (sprite is AnimatedSprite2D)
        {
            AnimatedSprite2D castedSprite = (AnimatedSprite2D)sprite;
            ShaderMaterial shadermat = (ShaderMaterial)castedSprite.Material;
            shadermat.SetShaderParameter("input_colour", colour);
            shadermat.SetShaderParameter("strength", strength);
        }
        else
        {
            throw new System.NotImplementedException("Weird sprite type");

        }
    }
}