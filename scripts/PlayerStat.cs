
using System;
using Godot;

public class PlayerStat
{
    public string name;
    public float baseValue;
    public Vector2 range;
    private float dynamicValue;
    public bool intChange;
    public bool invert; // Invert is true if increasing this stat is bad, and decreasing it is good
    public float changePolynomial;

    public PlayerStat(string _name, float _baseValue, Vector2 _range, bool _intChange = false, bool _invert = false, float _changePolynomial = 1)
    {
        name = _name;
        baseValue = _baseValue;
        range = _range;
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
        if (intChange)
        {
            dynamicValue += magnitude * (increase ? 1 : -1);
        }
        else
        {
            dynamicValue += magnitude * Mathf.Pow(baseValue, changePolynomial) * (increase ? 1 : -1);
        }

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
            float preview = dynamicValue + magnitude * (increase ? 1 : -1);
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
            float preview = dynamicValue + magnitude * baseValue * (increase ? 1 : -1);
            return string.Format("{0} {1} by {2:D}% ({3:n2} -> {4:n2}){5}", name, increase ? "Up" : "Down", (int)Math.Round(magnitude * 100), dynamicValue, preview, hitCap ? " (cap)" : "");

        }
    }

    public float GetDynamicVal()
    {
        return dynamicValue;
    }

}