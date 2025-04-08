using Godot;
using static Stats.PlayerStats;
using System;

public abstract class PlayerUpgrade
    {
        public string Name;
        protected string iconName;

        protected Condition appearCondition; // A condition that must be met before this upgrade will appear

        
        public virtual bool CheckCondition(){
            if (appearCondition is null){
                return true;
            }
            return appearCondition.CheckCondition();
        }


        public Texture2D GetUpgradeIcon()
        {
            return (Texture2D)GD.Load("res://custom assets/upgrade icons/" + iconName);
        }


        public abstract void Execute(float magnitude);

        
        public abstract string GetDescription(float magnitude);

        
        public abstract bool IsPositive(); // True if this upgrade is a good thing

    }
