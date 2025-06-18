using System.Collections.Generic;
using Godot;
using static Stats;

public class UnlockableWithBonus : Unlockable
{

    PlayerStat bonusStat;
    bool increase;
    bool multOrUpgrade; // If true, mult. If false, upgrade.

    float mag;


    public UnlockableWithBonus(string _name, StatSet _stats, bool _hasParticleEffect, List<VisualEffect> effects, PlayerStat _bonusStat, float _mag, bool _trueIfMultFalseIfUpgrade, bool _inc = true, Condition _condition = null) : base(_name, _stats, effects, _condition)
    {
        bonusStat = _bonusStat;
        multOrUpgrade = _trueIfMultFalseIfUpgrade;
        mag = _mag;
        increase = _inc;
    }


    public override void Unlock()
    {
        base.Unlock();
        if (multOrUpgrade)
        {
            bonusStat.AddMult(mag);
        }
        else
        {
            bonusStat.ApplyUpgrade(mag, increase);

        }
    }

}