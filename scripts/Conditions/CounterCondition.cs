using static Stats.Counters;
using Godot;

public class CounterCondition : Condition
{

    static Counter counter;
    int threshold;

    bool greaterThan;

    public CounterCondition(Counter _counter, int _threshold, bool _greaterThanEq = true)
    {
        counter = _counter;
        threshold = _threshold;
        greaterThan = _greaterThanEq;
    }

    public override bool CheckCondition()
    {
        int val = counter.Value;
        return greaterThan ? val >= threshold : val <= threshold;
    }
}