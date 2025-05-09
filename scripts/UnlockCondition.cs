using Godot;
using static Stats.PlayerStats.Unlocks;
using System;

public class UnlockCondition : Condition
{



	public Unlockable unlock;


	public bool equalTo;


	// If positive, stat needs to be greater than threshold. if negative, stat needs to be less than threshold
	public float threshold;

	public UnlockCondition(Unlockable _unlockable, bool _equalTo)
	{
		unlock = _unlockable;
		equalTo = _equalTo;
	}


	public override bool CheckCondition()
	{
		return unlock.unlocked == equalTo;
	}



}
