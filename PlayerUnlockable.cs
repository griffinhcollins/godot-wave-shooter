using Godot;
using static Stats.PlayerStats.Unlocks;
using System;

public class PlayerUnlockable : PlayerUpgrade
{

    public int unlockableID;

public PlayerUnlockable(int _unlockableID, string _Name, string _iconName, Condition _condition = null)
    {
        Name = _Name;
        iconName = _iconName;
        unlockableID = _unlockableID;
        if (_condition is not null)
        {
            appearCondition = _condition;
        }
    }


    public override void Execute(float magnitude)
    {
        ResetUnlockStats(unlockableID);
        DynamicUnlocks[unlockableID] = true;
    }

    public override bool CheckCondition(){
        // Don't show up as an option if it's already unlocked
        return base.CheckCondition() && !DynamicUnlocks[unlockableID];
    }

    public override string GetDescription(float magnitude)
    {
        return string.Format("Unlocks {0}", UnlockID.nameLookup[unlockableID]);
    }

    public override bool IsPositive()
    {
        return true; // Unlockables are always positive
    }
}