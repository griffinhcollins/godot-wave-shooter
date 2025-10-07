
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Godot;
using static RarityControl;

public class PlayerStat : Improvement
{
    public string name;
    public string description;
    public float baseValue;
    private Vector2 startingRange;
    private float dynamicValue;
    public bool intChange;
    public bool invert; // Invert is true if increasing this stat is bad, and decreasing it is good
    public Rarity rarity;
    public float changePolynomial;

    float multiplier;

    public Condition condition;

    public PlayerStat(string _name, string _description, float _baseValue, Vector2 _range, Rarity _rarity, bool _intChange = false, bool _invert = false, float _changePolynomial = 1, Condition _condition = null)
    {
        name = _name;
        description = _description;
        baseValue = _baseValue;
        startingRange = _range;
        intChange = _intChange;
        invert = _invert;
        rarity = _rarity;
        changePolynomial = _changePolynomial;
        if (_condition is not null)
        {
            condition = _condition;
        }
    }

    public Vector2 GetRange()
    {
        return startingRange * GetMult();
    }

    public float GetMult()
    {
        return multiplier;
    }

    public void AddMult(float amount)
    {
        multiplier += amount;
    }

    public void Reset()
    {
        multiplier = 1;
        dynamicValue = baseValue;
    }

    public void ApplyUpgrade(float magnitude, bool increase)
    {

        dynamicValue = CalculateStatUpgrade(magnitude, increase);
        Vector2 range = GetRange();
        if (dynamicValue > range.Y)
        {
            dynamicValue = range.Y;
        }
        if (dynamicValue < range.X)
        {
            dynamicValue = range.X;
        }
    }

    public string GetPreview(float magnitude, bool increase)
    {
        bool hitCap = false;
        Vector2 range = GetRange();
        if (intChange)
        {
            float preview = CalculateStatUpgrade(magnitude, increase);
            if (preview > range.Y)
            {
                hitCap = true;
                preview = range.Y;
            }
            if (preview < range.X)
            {
                hitCap = true;
                preview = range.X;
            }
            return string.Format("{0} {1} by {2:D}\n({3:D} -> {4:D}){5}", name, increase ? "Up" : "Down", (int)Mathf.Abs(preview - GetDynamicVal()), (int)GetDynamicVal(), (int)preview, hitCap ? " (cap)" : "");
        }
        else
        {
            float preview = CalculateStatUpgrade(magnitude, increase);
            if (preview > range.Y)
            {
                hitCap = true;
                preview = range.Y;
            }
            if (preview < range.X)
            {
                hitCap = true;
                preview = range.X;
            }
            return string.Format("{0} {1} by {2:D}%\n({3:n2} -> {4:n2}){5}", name, increase ? "Up" : "Down", (int)Math.Round(100 * Mathf.Abs(preview - GetDynamicVal()) / (GetDynamicVal() != 0 ? GetDynamicVal() : 1)), GetDynamicVal(), preview, hitCap ? " (cap)" : "");

        }
    }

    private float CalculateStatUpgrade(float magnitude, bool increase)
    {
        if (intChange)
        {
            return GetDynamicVal() + magnitude * (increase ? 1 : -1);
        }
        else
        {
            return GetDynamicVal() + magnitude * Mathf.Max(baseValue, 1) * (increase ? 1 : -1);
        }
    }

    public float GetDynamicVal()
    {
        return dynamicValue * GetMult();
    }

    public void Nullify()
    {
        dynamicValue = 0;
    }

    public PlayerStatUpgrade GenerateIncreasingUpgrade()
    {
        return new PlayerStatUpgrade(this, true, string.Format("{0}_up.png", name.ToLower()), condition);
    }

    public PlayerStatUpgrade GenerateDecreasingUpgrade()
    {

        return new PlayerStatUpgrade(this, false, string.Format("{0}_down.png", name.ToLower()), condition);
    }



    public override List<Improvement> GetPrerequisites(Condition c = null)
    {
        if (c is null)
        {
            if (condition is null)
            {
                return null;
            }
            c = condition;
        }
        if (c is StatCondition)
        {
            return new List<Improvement> { ((StatCondition)c).stat };
        }
        if (c is UnlockCondition)
        {
            return new List<Improvement> { ((UnlockCondition)c).GetUnlock() };
        }
        if (c is ConjunctCondition)
        {
            return ((ConjunctCondition)c).conditions.SelectMany(u => GetPrerequisites(u)).ToList();
        }
        if (c is CounterCondition)
        {
            return null;
        }
        throw new System.NotImplementedException();
    }

    public override string GetName()
    {
        return name;
    }



    public override string GetIconName()
    {
        return string.Format("{0}_{1}.png", name.ToLower(), invert ? "down" : "up");
    }
}