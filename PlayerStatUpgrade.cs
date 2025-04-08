using Godot;
using static Stats.PlayerStats;
using System;
using System.Collections.Generic;

public class PlayerStatUpgrade : PlayerUpgrade
{
    public PlayerStat stat; // ID of the stat this changes



    public PlayerStatUpgrade(PlayerStat _stat, bool _increasing, string _iconName, Condition _condition = null)
    {
        stat = _stat;
        positive = _increasing ^ stat.invert;
        iconName = _iconName;

        List<Condition> conditions = new List<Condition>();

        if (!Increasing())
        {
            conditions.Add(new StatCondition(stat, 1, true)); // A stat has to be >= 1 for an upgrade to show up that reduces it. Notably, this lets intchange stats hit 0
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
        stat.ApplyUpgrade(magnitude, Increasing());
    }

    public bool IntIncrease()
    {
        return stat.intChange;
    }

    public override string GetDescription(float magnitude)
    {
        return stat.GetPreview(magnitude, Increasing());
    }

    public override bool Increasing()
    {
        return positive ^ stat.invert;
    }

    public override bool IsPositive()
    {
        return positive;
    }
}
