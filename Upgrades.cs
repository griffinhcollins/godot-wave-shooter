using Godot;
using static Stats;
using System;

public static class Upgrades
{

	public static PlayerUpgrade dmgUp = new PlayerUpgrade(0, PlayerStats.ID.Damage, "Damage Up", false, true, true, "dmg_up.png");
	public static PlayerUpgrade dmgDown = new PlayerUpgrade(1, PlayerStats.ID.Damage, "Damage Down", false, false, false, "dmg_down.png");
	public static PlayerUpgrade firerateUp = new PlayerUpgrade(2, PlayerStats.ID.FireRate, "Firerate Up", false, true, true, "firerate_up.png");
	public static PlayerUpgrade firerateDown = new PlayerUpgrade(3, PlayerStats.ID.FireRate, "Firerate Down", false, false, false, "firerate_down.png");
	public static PlayerUpgrade hpUp = new PlayerUpgrade(4, PlayerStats.ID.HP, "HP Up", true, true, true, "hp_up.png");
	public static PlayerUpgrade hpDown = new PlayerUpgrade(5, PlayerStats.ID.HP, "HP Down", true, false, false, "hp_down.png");
	public static PlayerUpgrade hpRewardUp = new PlayerUpgrade(6, PlayerStats.ID.HPReward, "HP Interest Up", true, true, true, "hpreward_up.png");
	public static PlayerUpgrade hpRewardDown = new PlayerUpgrade(7, PlayerStats.ID.HPReward, "HP Interest Down", true, false, false, "hpreward_down.png");
	public static PlayerUpgrade droprateUp = new PlayerUpgrade(8, PlayerStats.ID.DropRate, "Coin Droprate Up", false, true, true, "moneyrate_up.png");
	public static PlayerUpgrade droprateDown = new PlayerUpgrade(9, PlayerStats.ID.DropRate, "Coin Droprate Down", false, false, false, "moneyrate_down.png");
	public static PlayerUpgrade multishotUp = new PlayerUpgrade(10, PlayerStats.ID.Multishot, "Multishot Up", false, true, true, "multishot_up.png");
	public static PlayerUpgrade multishotDown = new PlayerUpgrade(11, PlayerStats.ID.Multishot, "Multishot Down", false, false, false, "multishot_down.png");
	public static PlayerUpgrade spreadUp = new PlayerUpgrade(12, PlayerStats.ID.Spread, "Spread Up", false, false, true, "spread_up.png");
	public static PlayerUpgrade spreadDown = new PlayerUpgrade(13, PlayerStats.ID.Spread, "Spread Down", false, true, false, "spread_down.png");
	public static PlayerUpgrade shotspeedUp = new PlayerUpgrade(14, PlayerStats.ID.ShotSpeed, "Shotspeed Up", false, true, true, "shotspeed_up.png");
	public static PlayerUpgrade shotspeedDown = new PlayerUpgrade(15, PlayerStats.ID.ShotSpeed, "Shotspeed Down", false, false, false, "shotspeed_down.png");

	public static PlayerUpgrade[] allUpgrades = { dmgUp, dmgDown, firerateUp, firerateDown, hpUp, hpDown, hpRewardUp, hpRewardDown, droprateUp, droprateDown, multishotUp, multishotDown, spreadUp, spreadDown, shotspeedUp, shotspeedDown };

}