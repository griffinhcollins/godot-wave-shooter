
using Godot;

public class StaticColourChange : VisualEffect
{

    Color modulate;
    float lifeTime;

    Color prevColour; // Used for temporary effects

    public StaticColourChange(Color baseColour, float duration = Mathf.Inf)
    {
        modulate = baseColour;
        lifeTime = duration;
        prevColour = Colors.HotPink; // debug colour because colour doesn't null
    }


    public override string GetName()
    {
        return modulate.ToString();
    }

    public override void ImmediateEffect(Node2D parent)
    {
        applied = true;
        SetColour(parent, modulate);
    }

    public override void OngoingEffect(double delta, Node2D parent)
    {
        lifeTime -= (float)delta;
        if (applied && lifeTime <= 0 && prevColour != Color.Color8(0, 0, 0, 1))
        {
            applied = false;
            SetColour(parent, prevColour);
        }
        return;
    }

    private void SetColour(Node2D parent, Color colour)
    {
        Node2D sprite = parent.GetNode<Node2D>("MainSprite");
        if (prevColour == Colors.HotPink)
        {
            prevColour = sprite.Modulate;

        }
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