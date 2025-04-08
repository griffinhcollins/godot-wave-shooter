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
		foreach (PlayerStat stat in PlayerStats.allStats)
		{
			basicUpgrades.Add(new PlayerStatUpgrade(stat.ID, true, string.Format("{0}_up.png", stat.name.ToLower())));
			basicUpgrades.Add(new PlayerStatUpgrade(stat.ID, false, string.Format("{0}_down.png", stat.name.ToLower())));
		}
		GD.Print(basicUpgrades.Count);
	}

	// Stat Conditions, check if any of these can be added to the pool whenever a stat changes

	// If you reach high enough shotspeed and firerate, you get LASER BEAM as an option
	public static Condition laser = new ConjunctCondition(new List<Condition> { new StatCondition(PlayerStats.FireRate.ID, true, 1.5f, true), new StatCondition(PlayerStats.ShotSpeed.ID, true, 1.5f, true), new StatCondition(PlayerStats.Pierces.ID, true, 2, true) });


	// Upgrades that can only show up if you reduce your max HP to less than 1 (ie 0)
	public static StatCondition lich = new StatCondition(PlayerStats.MaxHP.ID, true, 1, false);

	public static PlayerUnlockable laserBeam = new PlayerUnlockable(PlayerStats.Unlocks.UnlockID.Laser, "Laser Beam", "unlock_laser.png", laser);

	public static PlayerUnlockable[] unlockables = { laserBeam };

	public static List<PlayerUpgrade> GetAllUpgrades(){
		List<PlayerUpgrade> allUpgrades = new List<PlayerUpgrade>();
		foreach (PlayerUpgrade statUpgrade in basicUpgrades)
		{
			allUpgrades.Add(statUpgrade);
		}
		foreach (PlayerUpgrade unlockable in unlockables)
		{
			allUpgrades.Add(unlockable);
		}
		return allUpgrades;
	}


}