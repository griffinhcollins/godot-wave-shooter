using Godot;
using static Upgrades;
using System;
using System.Linq;
using System.Collections.Generic;


public partial class UpgradeNode : Button
{

	Hud hud;

	HFlowContainer iconHolder;

	Dictionary<PlayerUpgrade, float> upgradeMagnitudes;


	int cost;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{


	}

	public List<PlayerUpgrade> Generate(List<PlayerUpgrade> upgradePool)
	{
		hud = (Hud)GetTree().GetNodesInGroup("HUD")[0];
		upgradeMagnitudes = new Dictionary<PlayerUpgrade, float>();
		iconHolder = GetNode<HFlowContainer>("IconHolder");
		upgradePool = Randomize(upgradePool);
		UpdateCost();
		return upgradePool;
	}

	List<PlayerUpgrade> Randomize(List<PlayerUpgrade> pool)
	{
		// How good this upgrade is, ie how much it should cost
		cost = 5;
		List<PlayerUpgrade> posUpgrades = pool.Where(u => u.IsPositive() && u.CheckCondition()).ToList<PlayerUpgrade>();

		List<PlayerStatUpgrade> negUpgrades = basicUpgrades.Where(u => !u.IsPositive() && u.CheckCondition()).ToList<PlayerStatUpgrade>();
		// Roll a positive upgrade
		int numPos = 1; // roll 1 upgrade
		int strength = 0;
		for (int i = 0; i < numPos; i++)
		{
			PlayerUpgrade newPos = posUpgrades[GD.RandRange(0, posUpgrades.Count - 1)];
			// Make sure this upgrade isn't repeated in this shop
			pool.Remove(newPos);
			AddIcon(newPos);
			if (newPos is PlayerStatUpgrade)
			{
				strength = GD.RandRange(0, 2);
				upgradeMagnitudes.Add(newPos, CalculateMagnitude(((PlayerStatUpgrade)newPos).IntIncrease(), strength));

				// Make sure this stat isn't repeated within this upgrade
				posUpgrades = posUpgrades.Where(u => u is PlayerStatUpgrade ? ((PlayerStatUpgrade)u).stat != ((PlayerStatUpgrade)newPos).stat : true).ToList();
				negUpgrades = negUpgrades.Where(u => u is PlayerStatUpgrade ? ((PlayerStatUpgrade)u).stat != ((PlayerStatUpgrade)newPos).stat : true).ToList();

			}
			else
			{
				// upgrade is an unlock
				strength = 3;
				cost += strength * 3;
				upgradeMagnitudes.Add(newPos, 1);
				RarityColour(strength);
				return pool;
			}
			cost += strength * 3;
		}
		RarityColour(strength);
		// Have 0-1 negative upgrades, more likely if the positive upgrade is strong
		float negChance = (GD.Randf() + 2 * strength) / 5;
		if (GD.Randf() <= negChance)
		{
			PlayerStatUpgrade newNeg = negUpgrades[GD.RandRange(0, negUpgrades.Count - 1)];
			AddIcon(newNeg);
			int negStrength = GD.RandRange(0, 2);
			upgradeMagnitudes.Add(newNeg, CalculateMagnitude(newNeg.IntIncrease(), negStrength));
			cost -= negStrength * 3;
			negUpgrades = negUpgrades.Where(u => u.stat != newNeg.stat).ToList();
		}
		return pool;
	}



	private void RarityColour(int rarity)
	{

		Color backgroundColour = new Color();


		switch (rarity)
		{
			case 0:
				backgroundColour = new Color(1, 1, 1);
				break;
			case 1:
				backgroundColour = new Color(0.8f, 0.8f, 1);
				break;
			case 2:
				backgroundColour = new Color(0.8f, 0.4f, 1f);
				break;
			case 3:
				backgroundColour = new Color(1, 0.8f, 0);

				break;
		}

		SelfModulate = backgroundColour;
	}

	private void OnMouseOver()
	{
		string infoMessage = "";
		foreach (PlayerUpgrade upgrade in upgradeMagnitudes.Keys)
		{
			float mag = upgradeMagnitudes[upgrade];
			infoMessage += upgrade.GetDescription(mag) + ", ";
		}
		infoMessage = infoMessage.Remove(infoMessage.Length - 2);
		hud.ShowMessage(infoMessage, false);
	}

	private void OnMouseLeave()
	{
		hud.HideMessage();
	}

	private void OnClicked()
	{
		if (!((Player)GetTree().GetNodesInGroup("player")[0]).ChargeMoney(cost))
		{
			return;
		}

		foreach (PlayerUpgrade upgrade in upgradeMagnitudes.Keys)
		{
			upgrade.Execute(upgradeMagnitudes[upgrade]);
		}

		QueueFree();

	}

	// strength can be 0 (standard), 1 (extended) or 2+ (extreme)
	private float CalculateMagnitude(bool intChange, int strength)
	{
		if (intChange)
		{
			return Math.Max(strength, 1);

		}
		else
		{
			return (0.24f + GD.Randf() / 4) * (1 + strength / 2);
		}


	}

	void UpdateCost()
	{
		GetNode<Label>("Cost").Text = string.Format("${0}", cost);
	}

	private void AddIcon(PlayerUpgrade upgrade)
	{
		TextureRect textureRect = new TextureRect();
		textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		textureRect.CustomMinimumSize = new Vector2(45, 45);
		textureRect.Texture = upgrade.GetUpgradeIcon();
		iconHolder.AddChild(textureRect);
	}

}
