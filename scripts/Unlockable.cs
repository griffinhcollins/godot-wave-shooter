using System.Collections.Generic;
using System.Linq;
using Godot;
using static Stats;
public class Unlockable : Prerequisite
{
    public bool unlocked { get; private set; }
    public string name;

    // The condition after which this unlockable can show up in the shop
    public Condition condition;

    public StatSet associatedStats;

    public List<PlayerStat> prerequisites;

    public Unlockable(string _name, StatSet _stats, Condition _condition = null)
    {
        name = _name;
        associatedStats = _stats;
        if (_condition is not null)
        {
            condition = _condition;

        }
        associatedStats.AddCondition(new UnlockCondition(this, true).And(condition));

    }


    public void Reset()
    {
        unlocked = false;
        // Set everything to 0 when they aren't unlocked yet
        associatedStats.SetToZero();
    }

    public virtual void Unlock()
    {
        unlocked = true;
        associatedStats.SetToDefaultStartingValues();
    }



    public List<Prerequisite> GetPrerequisites(Condition c = null)
    {
        if (c is null)
        {
            if (condition is null){
                return null;
            }
            c = condition;
        }
        if (c is StatCondition)
        {
            return new List<Prerequisite> { ((StatCondition)c).stat };
        }
        if (c is UnlockCondition)
        {
            return new List<Prerequisite> { ((UnlockCondition)c).unlock };
        }
        if (c is ConjunctCondition)
        {
            return ((ConjunctCondition)c).conditions.SelectMany(u => GetPrerequisites(u)).ToList();
        }
        throw new System.NotImplementedException();
    }

    public string GetName()
    {
        return name;
    }

    public string GetIconName()
    {
        return string.Format("{0}.png", name.ToLower());
    }
}