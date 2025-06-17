
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

    public override void ImmediateEffect(IAffectedByVisualEffects parent)
    {
        applied = true;
        parent.AddStaticColour(modulate);
    }

    public override void OngoingEffect(double delta, IAffectedByVisualEffects parent)
    {
        lifeTime -= (float)delta;
        if (applied && lifeTime <= 0)
        {
            applied = false;
            parent.RemoveStaticColour(modulate);
        }
        return;
    }

    
}