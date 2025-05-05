using Godot;
using static Stats;
using System;

public class StatCondition : Condition
{



	public PlayerStat stat;


	public bool greaterThan;


	// If positive, stat needs to be greater than threshold. if negative, stat needs to be less than threshold
	public float threshold;

	public StatCondition(PlayerStat _stat, float _threshold, bool _greaterThan)
	{
		stat = _stat;
		threshold = _threshold;
		greaterThan = _greaterThan;
	}


	public override bool CheckCondition()
	{
		GD.Print(threshold);
		GD.Print(stat.name);
		float statVal = stat.GetDynamicVal();

		return greaterThan ? statVal > threshold : statVal < threshold;
	}



}
