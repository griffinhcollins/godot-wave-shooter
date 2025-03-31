using Godot;
using static Stats;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Upgrades
{

	// Upgrades that can always show up
	public static PlayerStatUpgrade dmgUp = new PlayerStatUpgrade(0, PlayerStats.ID.Damage, "Damage Up", false, true, true, "dmg_up.png");
	public static PlayerStatUpgrade dmgDown = new PlayerStatUpgrade(1, PlayerStats.ID.Damage, "Damage Down", false, false, false, "dmg_down.png");
	public static PlayerStatUpgrade firerateUp = new PlayerStatUpgrade(2, PlayerStats.ID.FireRate, "Firerate Up", false, true, true, "firerate_up.png");
	public static PlayerStatUpgrade firerateDown = new PlayerStatUpgrade(3, PlayerStats.ID.FireRate, "Firerate Down", false, false, false, "firerate_down.png");
	public static PlayerStatUpgrade hpUp = new PlayerStatUpgrade(4, PlayerStats.ID.HP, "HP Up", true, true, true, "hp_up.png");
	public static PlayerStatUpgrade hpDown = new PlayerStatUpgrade(5, PlayerStats.ID.HP, "HP Down", true, false, false, "hp_down.png");
	public static PlayerStatUpgrade hpRewardUp = new PlayerStatUpgrade(6, PlayerStats.ID.HPReward, "HP Interest Up", true, true, true, "hpreward_up.png");
	public static PlayerStatUpgrade hpRewardDown = new PlayerStatUpgrade(7, PlayerStats.ID.HPReward, "HP Interest Down", true, false, false, "hpreward_down.png");
	public static PlayerStatUpgrade droprateUp = new PlayerStatUpgrade(8, PlayerStats.ID.DropRate, "Coin Droprate Up", false, true, true, "moneyrate_up.png");
	public static PlayerStatUpgrade droprateDown = new PlayerStatUpgrade(9, PlayerStats.ID.DropRate, "Coin Droprate Down", false, false, false, "moneyrate_down.png");
	public static PlayerStatUpgrade multishotUp = new PlayerStatUpgrade(10, PlayerStats.ID.Multishot, "Multishot Up", false, true, true, "multishot_up.png");
	public static PlayerStatUpgrade multishotDown = new PlayerStatUpgrade(11, PlayerStats.ID.Multishot, "Multishot Down", false, false, false, "multishot_down.png");
	public static PlayerStatUpgrade spreadUp = new PlayerStatUpgrade(12, PlayerStats.ID.Spread, "Spread Up", false, false, true, "spread_up.png");
	public static PlayerStatUpgrade spreadDown = new PlayerStatUpgrade(13, PlayerStats.ID.Spread, "Spread Down", false, true, false, "spread_down.png");
	public static PlayerStatUpgrade shotspeedUp = new PlayerStatUpgrade(14, PlayerStats.ID.ShotSpeed, "Shotspeed Up", false, true, true, "shotspeed_up.png");
	public static PlayerStatUpgrade shotspeedDown = new PlayerStatUpgrade(15, PlayerStats.ID.ShotSpeed, "Shotspeed Down", false, false, false, "shotspeed_down.png");

	public static PlayerStatUpgrade[] basicUpgrades = { dmgUp, dmgDown, firerateUp, firerateDown, hpUp, hpDown, hpRewardUp, hpRewardDown, droprateUp, droprateDown, multishotUp, multishotDown, spreadUp, spreadDown, shotspeedUp, shotspeedDown };

	// Stat Conditions, check if any of these can be added to the pool whenever a stat changes

	// If you reach high enough shotspeed and firerate, you get LASER BEAM as an option
	public static Condition laser = new ConjunctCondition(new List<Condition> { new StatCondition(PlayerStats.ID.FireRate, true, 1, true), new StatCondition(PlayerStats.ID.ShotSpeed, true, 1, true) });


	// Upgrades that can only show up if you reduce your max HP to less than 1 (ie 0)
	public static StatCondition lich = new StatCondition(PlayerStats.ID.HP, true, 1, false);

	public static PlayerUnlockable laserBeam = new PlayerUnlockable(16, PlayerStats.Unlocks.UnlockID.Laser, "Laser Beam", true, "unlock_laser.png", laser);

	public static PlayerUnlockable[] unlockables = { laserBeam };


	public static List<PlayerUpgrade> allUpgrades = basicUpgrades.ToList<PlayerUpgrade>().Concat(unlockables.ToList<PlayerUpgrade>()).ToList();


}