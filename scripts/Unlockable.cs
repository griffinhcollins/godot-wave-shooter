using static Stats;
public class Unlockable
{
    public bool unlocked { get; private set; }
    public string name;

    // The condition after which this unlockable can show up in the shop
    public Condition condition;

    public StatSet associatedStats;

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

    public void Unlock()
    {
        unlocked = true;
        associatedStats.SetToDefaultStartingValues();
    }



}