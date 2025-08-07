using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

public class ConjunctCondition : Condition
{

    public List<Condition> conditions;

    public bool and;

    public ConjunctCondition(List<Condition> _conditions, bool _and = true)
    {
        conditions = _conditions;
        and = _and;
    }

    public override bool CheckCondition()
    {
        if (and)
        {
            return conditions.Aggregate(true, (acc, x) => acc && x.CheckCondition());

        }
        else
        {
            return conditions.Aggregate(false, (acc, x) => acc || x.CheckCondition());
            
        }
    }




}
