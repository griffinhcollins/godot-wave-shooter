using Godot;
using static Stats.PlayerStats;
using System;
using System.Collections.Generic;

public class PlayerStatUpgrade : PlayerUpgrade
{
    public int statID; // ID of the stat this changes
    public bool intChange; // True if this upgrade increases by an integer value (like HP), false if it increases as a percentage
    public bool increase; // True if this upgrade should increase its given stat


    public PlayerStatUpgrade(int _ID, int _statID, string _Name, bool _intChange, bool _positive, bool _increase, string _iconName, Condition _condition = null)
    {
        ID = _ID;
        statID = _statID;
        Name = _Name;
        intChange = _intChange;
        positive = _positive;
        increase = _increase;
        iconName = _iconName;

        List<Condition> conditions = new List<Condition>();

        if (!increase){

            conditions.Add(new StatCondition(statID, true, 1, true)); // A stat has to be >= 1 for an upgrade to show up that reduces it. Notably, this lets intchange stats hit 0


        }

        if (_condition is not null) 
        {
            conditions.Add(_condition);
        }

        if (conditions.Count > 0){
            appearCondition = new ConjunctCondition(conditions);
        }
    }



    public override void Execute(float magnitude)
    {
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
            return string.Format("{0} by {1:D} ({2:D})", Name, (int)Math.Round(magnitude), (int)preview);
        }
        else
        {
            float preview = DynamicStats[statID] + magnitude * BaseStats[statID] * (increase ? 1 : -1);
            return string.Format("{0} by {1:D}% ({2:n2})", Name, (int)Math.Round(magnitude * 100), preview);

        }
    }
}
