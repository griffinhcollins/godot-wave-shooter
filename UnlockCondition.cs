using Godot;
using static Stats.PlayerStats.Unlocks;
using System;

public class UnlockCondition : Condition
{



	public Unlockable u;


	public bool equalTo;


	// If positive, stat needs to be greater than threshold. if negative, stat needs to be less than threshold
	public float threshold;

	public UnlockCondition(Unlockable _unlockable, bool _equalTo)
	{
		u = _unlockable;
		equalTo = _equalTo;
	}


	public override bool CheckCondition()
	{
		return u.unlocked == equalTo;
	}



}
