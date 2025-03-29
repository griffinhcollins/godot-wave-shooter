using Godot;
using static Stats.PlayerStats;
using System;

public abstract class PlayerUpgrade
    {
        public int ID;
        public string Name;
		public bool positive; // True if this upgrade should be seen as a good thing
        private string iconName;

        Condition appearCondition; // A condition that must be met before this upgrade will appear

        
        public bool CheckCondition(){
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

    }
