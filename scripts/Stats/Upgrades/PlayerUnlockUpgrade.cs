using Godot;
using static Stats.PlayerStats.Unlocks;
using System;

public class PlayerUnlockUpgrade : PlayerUpgrade
{

    public Unlockable unlock;

    public PlayerUnlockUpgrade(Unlockable _unlockable)
    {
        improvement = _unlockable;
        unlock = _unlockable;
        iconName = string.Format("{0}.png", unlock.GetName().ToLower());
        if (unlock.condition is not null)
        {
            appearCondition = unlock.condition;
        }
    }


    public override void Execute(float magnitude)
    {
        unlock.Unlock();
    }

    public override bool CheckCondition()
    {
        // Don't show up as an option if it's already unlocked
        return base.CheckCondition() && !unlock.unlocked;
    }

    public override string GetMechanicalChange(float magnitude)
    {
        return string.Format("Permanently unlocks {0}, associated upgrades will appear in the shop.", unlock.GetName());
    }

    public override bool IsPositive()
    {
        return true; // Unlockables are always positive
    }

    public override bool Increasing()
    {
        // This should never come up
        throw new Exception("Why are you asking if an upgrade is increasing?");
    }

    public override string GetName()
    {
        return unlock.GetName();
    }

    public override string GetWordyDescription()
    {
        return unlock.description;
    }
}