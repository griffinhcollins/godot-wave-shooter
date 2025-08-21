using System.Collections.Generic;
using Godot;
using static Stats;

public class UnlockableWithBonus : Unlockable
{

    Dictionary<PlayerStatUpgrade, float> bonusUpgrades;

    public UnlockableWithBonus(string _name, string _description, StatSet _stats, Dictionary<PlayerStatUpgrade, float> _bonusUpgrades, List<VisualEffect> effects = null, Condition _condition = null) : base(_name, _description, _stats, effects, _condition)
    {
        bonusUpgrades = _bonusUpgrades;
    }


    public override void Unlock()
    {
        base.Unlock();
        foreach (PlayerStatUpgrade u in bonusUpgrades.Keys)
        {
            u.Execute(bonusUpgrades[u]);
        }
    }

}