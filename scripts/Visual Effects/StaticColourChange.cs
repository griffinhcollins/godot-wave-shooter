
using Godot;

public class StaticColourChange : VisualEffect
{

    Color modulate;

    public StaticColourChange(Color baseColour)
    {
        modulate = baseColour;
    }


    public override string GetName()
    {
        return modulate.ToString();
    }

    public override void ImmediateEffect(Node2D parent)
    {
        Node2D sprite = parent.GetNode<Node2D>("MainSprite");
        if (sprite is Sprite2D)
        {
            sprite.Modulate = modulate;
        }
        else if (sprite is AnimatedSprite2D)
        {
            sprite.Modulate = modulate;
        }
        else
        {
            throw new System.NotImplementedException("Weird sprite type");

        }
    }

    public override void OngoingEffect(double delta, Node2D parent)
    {
        return;
    }
}