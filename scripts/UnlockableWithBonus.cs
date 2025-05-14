using Godot;
using static Stats;

public class UnlockableWithBonus : Unlockable
{

    PlayerStat bonusStat;
    float bonusMagnitude;
    bool increase;

    public UnlockableWithBonus(string _name, StatSet _stats, PlayerStat _bonusStat, float _mag, bool _inc, Condition _condition = null) : base(_name, _stats, _condition)
    {
        bonusStat = _bonusStat;
        bonusMagnitude = _mag;
        increase = _inc;
    }


    public override void Unlock()
    {
        base.Unlock();
        bonusStat.ApplyUpgrade(bonusMagnitude, true);
    }

}