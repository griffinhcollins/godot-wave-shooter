using Godot;
using static Stats;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Upgrades
{

	static List<PlayerUpgrade> allUpgrades;


	// Generate all upgrades that can be purchased (ie their conditions are met)
	public static List<PlayerUpgrade> GetAvailableUpgrades()
	{
		allUpgrades = PlayerStats.GetAllUpgrades();
		return allUpgrades.Where(u => u.CheckCondition()).ToList();
	}






}