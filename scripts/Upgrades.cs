using Godot;
using static Stats;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Upgrades
{

	// Upgrades that can always show up


	public static List<PlayerStatUpgrade> basicUpgrades = new List<PlayerStatUpgrade>();

	public static void GenerateUpgrades()
	{
		basicUpgrades.Clear();
		foreach (PlayerStat stat in PlayerStats.upgradeableStats)
		{
			basicUpgrades.Add(new PlayerStatUpgrade(stat, true, string.Format("{0}_up.png", stat.name.ToLower())));
			basicUpgrades.Add(new PlayerStatUpgrade(stat, false, string.Format("{0}_down.png", stat.name.ToLower())));
		}
	}

	// Stat Conditions, check if any of these can be added to the pool whenever a stat changes

	


	public static List<PlayerUpgrade> GetAllUpgrades()
	{
		List<PlayerUpgrade> allUpgrades = new List<PlayerUpgrade>();
		foreach (PlayerUpgrade statUpgrade in basicUpgrades)
		{
			allUpgrades.Add(statUpgrade);
		}
		foreach (Unlockable unlockable in PlayerStats.Unlocks.allUnlockables.Where(u => !u.unlocked))
		{
			allUpgrades.Add(new PlayerUnlockUpgrade(unlockable));
		}
		return allUpgrades;
	}


}