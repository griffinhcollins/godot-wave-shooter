using Godot;
using static Stats.PlayerStats;
using System;

public class PlayerUpgrade
    {
        public int ID;
		public int statID; // ID of the stat this changes
        public string Name;
        public bool intChange; // True if this upgrade increases by an integer value (like HP), false if it increases as a percentage
		public bool positive; // True if this upgrade should be seen as a good thing
		public bool increase; // True if this upgrade should increase its given stat
        private string iconName;

        public PlayerUpgrade(int _ID, int _statID, string _Name, bool _intChange, bool _positive, bool _increase, string _iconName){
            ID = _ID;
			statID = _statID;
			Name = _Name;
			intChange = _intChange;
			positive = _positive;
			increase = _increase;
			iconName = _iconName;
        }



        public Texture2D GetUpgradeIcon()
        {
            return (Texture2D)GD.Load("res://custom assets/upgrade icons/" + iconName);
        }

        public void Execute(float magnitude)
        {
			GD.Print(statID);
			GD.Print(DynamicStats.Length);
            if (intChange)
            {
                DynamicStats[statID] += magnitude * (increase ? 1 : -1);
            }
            else
            {
                DynamicStats[statID] += magnitude * BaseStats[statID] * (increase ? 1 : -1);
            }
        }
    }
