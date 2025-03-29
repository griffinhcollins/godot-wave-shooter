using Godot;
using static Stats.PlayerStats;
using System;

public class PlayerStatUpgrade : PlayerUpgrade
{
    public int statID; // ID of the stat this changes
    public bool intChange; // True if this upgrade increases by an integer value (like HP), false if it increases as a percentage
    public bool increase; // True if this upgrade should increase its given stat
    private string iconName;

    Condition appearCondition; // A condition that must be met before this upgrade will appear

    public PlayerStatUpgrade(int _ID, int _statID, string _Name, bool _intChange, bool _positive, bool _increase, string _iconName, Condition _condition = null)
    {
        ID = _ID;
        statID = _statID;
        Name = _Name;
        intChange = _intChange;
        positive = _positive;
        increase = _increase;
        iconName = _iconName;
        if (_condition is not null)
        {
            appearCondition = _condition;
        }
    }



    public override void Execute(float magnitude)
    {
        GD.Print(statID);
        GD.Print(DynamicStats.Length);
        if (intChange)
        {
            DynamicStats[statID] += magnitude * (increase ? 1 : -1);
        }
        else
        {
            DynamicStats[statID] += magnitude * BaseStats[statID] * (increase ? 1 : -1);
        }
    }

    public override string GetDescription(float magnitude)
    {
        if (intChange)
        {
            float preview = DynamicStats[statID] + magnitude * (increase ? 1 : -1); ;
            return string.Format("{0} by {1:D} ({2:D}), ", Name, (int)Math.Round(magnitude), (int)preview);
        }
        else
        {
            float preview = DynamicStats[statID] + magnitude * BaseStats[statID] * (increase ? 1 : -1);
            return string.Format("{0} by {1:D}% ({2:n2}), ", Name, (int)Math.Round(magnitude * 100), preview);

        }
    }
}
