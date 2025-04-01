using Godot;
using static Stats.PlayerStats.Unlocks;
using System;

public class UnlockCondition : Condition
{



	public int unlockableID;


	public bool equalTo;


	// If positive, stat needs to be greater than threshold. if negative, stat needs to be less than threshold
	public float threshold;

	public UnlockCondition(int _unlockID, bool _equalTo)
	{
		unlockableID = _unlockID;
		equalTo = _equalTo;
	}


	public override bool CheckCondition()
	{
		return DynamicUnlocks[unlockableID] == equalTo;
	}



}
