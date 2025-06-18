
// Applies to anything that can be a prerequisite to unlocking another upgrade. Currently used only for the upgrade tree
using System.Collections.Generic;
using Godot;

public abstract class Improvement
{


    protected List<VisualEffect> visEffects;

    public abstract List<Improvement> GetPrerequisites(Condition condition = null);

    public abstract string GetName();

    public abstract string GetIconName();


    public List<VisualEffect> GetVisualEffects()
    {
        if (visEffects is null)
        {
            return new();
        }
        return visEffects;
    }

    public void AddVisualEffect(VisualEffect newEffect)
    {
        if (visEffects is null)
        {
            visEffects = new();
        }
        visEffects.Add(newEffect);

    }


}