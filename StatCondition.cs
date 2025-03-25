using Godot;
using static Stats;
using System;

public class StatCondition : Condition
{



	public int statID;

	public bool playerStat;

	public bool greaterThan;


	// If positive, stat needs to be greater than threshold. if negative, stat needs to be less than threshold
	public float threshold;

	public StatCondition(int _statID, bool _playerStat, float _threshold, bool _greaterThan)
	{
		statID = _statID;
		playerStat = _playerStat;
		threshold = _threshold;
		greaterThan = _greaterThan;
	}


	public override bool CheckCondition()
	{
		float stat = playerStat ? PlayerStats.DynamicStats[statID] : EnemyStats.DynamicStats[statID];

		return greaterThan ? stat > threshold : stat < threshold;
	}



}
