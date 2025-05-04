using Godot;
using static Stats;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Upgrades
{


	// Generate all upgrades that can be purchased (ie their conditions are met)
	public static List<PlayerUpgrade> GetAvailableUpgrades()
	{
		return PlayerStats.GetAllUpgrades().Where(u => u.CheckCondition()).ToList();
	}


	



}