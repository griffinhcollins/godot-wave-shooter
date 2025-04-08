using Godot;
using static Stats.PlayerStats;
using System;
using System.Collections.Generic;

public class PlayerStatUpgrade : PlayerUpgrade
{
    public int statID; // ID of the stat this changes

    bool increase; // Whether this upgrade increases or decreases the given stat (unrelated to whether that's positive or negative)


    public PlayerStatUpgrade(int _statID, bool _increase, string _iconName, Condition _condition = null)
    {
        statID = _statID;
        increase = _increase;
        iconName = _iconName;

        List<Condition> conditions = new List<Condition>();

        if (!increase)
        {
            conditions.Add(new StatCondition(statID, true, 1, true)); // A stat has to be >= 1 for an upgrade to show up that reduces it. Notably, this lets intchange stats hit 0
        }

        if (_condition is not null)
        {
            conditions.Add(_condition);
        }

        if (conditions.Count > 0)
        {
            appearCondition = new ConjunctCondition(conditions);
        }
    }



    public override void Execute(float magnitude)
    {
        allStats[statID].ApplyUpgrade(magnitude, increase);
    }

    public bool IntIncrease()
    {
        return allStats[statID].intChange;
    }

    public override string GetDescription(float magnitude)
    {
        PlayerStat stat = allStats[statID];
        return stat.GetPreview(magnitude, increase);
    }

    public override bool IsPositive()
    {
        return increase ^ allStats[statID].invert;
    }
}
