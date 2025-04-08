
using System;
using Godot;

public class PlayerStat
{
    public int ID;
    public string name;
    public float baseValue;
    private float dynamicValue;
    public bool intChange;
    public bool invert; // Invert is true if increasing this stat is bad, and decreasing it is good
    public float changePolynomial;

    public PlayerStat(int _ID, string _name, float _baseValue, bool _intChange = false, bool _invert = false, float _changePolynomial = 1)
    {
        ID = _ID;
        name = _name;
        baseValue = _baseValue;
        intChange = _intChange;
        invert = _invert;
        changePolynomial = _changePolynomial;
    }

    public void Reset()
    {
        dynamicValue = baseValue;
    }

    public void ApplyUpgrade(float magnitude, bool increase)
    {
        float changeAmount = 0;
        if (intChange)
        {
            changeAmount= magnitude * (invert ? 1 : -1);
        }
        else
        {
            changeAmount= magnitude * Mathf.Pow(baseValue, changePolynomial) * (invert ? 1 : -1);
        }

        dynamicValue += changeAmount * (increase ? 1 : -1);
    }

    public string GetPreview(float magnitude, bool increase)
    {
        if (intChange)
        {
            float preview = dynamicValue + magnitude * (increase ? 1 : -1); ;
            return string.Format("{0} {1} by {2:D} ({3:D})", name, increase ? "Up" : "Down", (int)Math.Round(magnitude), (int)preview);
        }
        else
        {
            float preview = dynamicValue + magnitude * baseValue * (increase ? 1 : -1);
            return string.Format("{0} {1} by {2:D}% ({3:n2})", name, increase ? "Up" : "Down", (int)Math.Round(magnitude * 100), preview);

        }
    }

    public float GetDynamicVal()
    {
        return dynamicValue;
    }

}