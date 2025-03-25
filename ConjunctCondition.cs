using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

public class ConjunctCondition : Condition
{

    public List<Condition> conditions;

    public bool and;

    public ConjunctCondition(List<Condition> _conditions)
    {
        conditions = _conditions;
    }

    public override bool CheckCondition()
    {
        return conditions.Aggregate(true, (acc, x) => acc && x.CheckCondition());
    }




}
