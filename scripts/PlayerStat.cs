
using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Godot;

public class PlayerStat : Prerequisite
{
    public string name;
    public float baseValue;
    public Vector2 range;
    private float dynamicValue;
    public bool intChange;
    public bool invert; // Invert is true if increasing this stat is bad, and decreasing it is good
    public float changePolynomial;

    public Condition condition;

    public PlayerStat(string _name, float _baseValue, Vector2 _range, bool _intChange = false, bool _invert = false, float _changePolynomial = 1, Condition _condition = null)
    {
        name = _name;
        baseValue = _baseValue;
        range = _range;
        intChange = _intChange;
        invert = _invert;
        changePolynomial = _changePolynomial;
        if (_condition is not null)
        {
            condition = _condition;
        }
    }

    public void Reset()
    {
        dynamicValue = baseValue;
    }

    public void ApplyUpgrade(float magnitude, bool increase)
    {

        dynamicValue = CalculateStatUpgrade(magnitude, increase);

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
            return string.Format("{0} {1} by {2:D} ({3:D} -> {4:D}){5}", name, increase ? "Up" : "Down", (int)Math.Round(magnitude), (int)dynamicValue, (int)preview, hitCap ? " (cap)" : "");
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
            return string.Format("{0} {1} by {2:D}% ({3:n2} -> {4:n2}){5}", name, increase ? "Up" : "Down", (int)Math.Round(magnitude * 100), dynamicValue, preview, hitCap ? " (cap)" : "");

        }
    }

    private float CalculateStatUpgrade(float magnitude, bool increase)
    {
        if (intChange)
        {
            return dynamicValue + magnitude * (increase ? 1 : -1);
        }
        else
        {
            return dynamicValue + magnitude * Mathf.Max(baseValue, 1) * (increase ? 1 : -1);
        }
    }

    public float GetDynamicVal()
    {
        return dynamicValue;
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


    public PlayerStat GetStat()
    {
        return this;
    }

    public Unlockable GetUnlockable()
    {
        return null;
    }

    public List<Prerequisite> GetPrerequisites(Condition c = null)
    {
        if (c is null)
        {
            if (condition is null){
                return null;
            }
            c = condition;
        }
        if (c is StatCondition)
        {
            return new List<Prerequisite> { ((StatCondition)c).stat };
        }
        if (c is UnlockCondition)
        {
            return new List<Prerequisite> { ((UnlockCondition)c).unlock };
        }
        if (c is ConjunctCondition)
        {
            return ((ConjunctCondition)c).conditions.SelectMany(u => GetPrerequisites(u)).ToList();
        }
        throw new System.NotImplementedException();
    }

    public string GetName()
    {
        return name;
    }



    public string GetIconName()
    {
        return string.Format("{0}_up.png", name.ToLower());
    }
}