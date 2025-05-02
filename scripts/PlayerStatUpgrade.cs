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
            conditions.Add(new StatCondition(stat, stat.range.X, true));
        }
        else
        {

            conditions.Add(new StatCondition(stat, stat.range.Y, false));
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
