using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IAffectedByVisualEffects
{
    public List<VisualEffect> visualEffects { get; set; }

    Dictionary<StaticColourChange, float> staticColours { get; set; } // Colour changes and their priorities

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

    public void AddStaticColour(Node2D parent, StaticColourChange colourChange)
    {
        if (staticColours is null)
        {
            staticColours = new();
        }
        staticColours[colourChange] = colourChange.priority;
        SetColour(parent, GetStaticColour());
    }

    public void RemoveStaticColour(Node2D parent, StaticColourChange colourChange)
    {
        if (staticColours is null)
        {
            return;
        }
        staticColours.Remove(colourChange);
        SetColour(parent, GetStaticColour());
    }

    public StaticColourChange GetStaticColour()
    {
        if (staticColours is null || staticColours.Count == 0)
        {
            return null;
        }
        StaticColourChange maxPriority = staticColours.Aggregate(staticColours.First().Key, (c1, c2) => staticColours[c1] > c2.Value ? c1 : c2.Key);
        return maxPriority;
    }

    private void SetColour(Node2D parent, StaticColourChange colourChange)
    {
        Node2D sprite = parent.GetNode<Node2D>("MainSprite");
        ShaderMaterial shadermat;
        if (sprite is Sprite2D)
        {
            Sprite2D castedSprite = (Sprite2D)sprite;
            shadermat = (ShaderMaterial)castedSprite.Material;
        }
        else if (sprite is AnimatedSprite2D)
        {
            AnimatedSprite2D castedSprite = (AnimatedSprite2D)sprite;
            shadermat = (ShaderMaterial)castedSprite.Material;
        }
        else
        {
            throw new System.NotImplementedException("Weird sprite type");

        }

        if (colourChange is null)
        {
            shadermat.SetShaderParameter("input_colour", Colors.White);
        }
        else
        {
            Vector4 input_colour = new Vector4(colourChange.modulate.R, colourChange.modulate.G, colourChange.modulate.B, colourChange.strength);
            shadermat.SetShaderParameter("input_colour", input_colour);

        }

    }
}