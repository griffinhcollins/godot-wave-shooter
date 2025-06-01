using Godot;
using static Stats;
using System;
using System.Collections.Generic;

public abstract class Condition
{



	public abstract bool CheckCondition();

	public virtual Condition And(Condition condition)
	{
		if (condition is null){
			return this;
		}
		return new ConjunctCondition(new List<Condition> {this, condition});
	}



}
