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

		hud = (Hud)GetTree().GetNodesInGroup("HUD")[0];
		upgradeMagnitudes = new Dictionary<PlayerUpgrade, float>();
		iconHolder = GetNode<HFlowContainer>("IconHolder");
		Randomize();
		UpdateCost();
	}

	void Randomize()
	{
		// How good this upgrade is, ie how much it should cost
		cost = 5;

		PlayerUpgrade[] posUpgrades = basicUpgrades.Where(u => u.positive).ToArray();
		PlayerUpgrade[] negUpgrades = basicUpgrades.Where(u => !u.positive).ToArray();
		// Roll 1-2 positive upgrades
		int numPos = GD.RandRange(1, 2);
		for (int i = 0; i < numPos; i++)
		{
			PlayerUpgrade newPos = posUpgrades[GD.RandRange(0, posUpgrades.Length - 1)];
			AddIcon(newPos);
			int strength = GD.RandRange(0, 2);
			upgradeMagnitudes.Add(newPos, CalculateMagnitude(newPos.intChange, strength));
			cost += strength * 3;
			// Eliminate any other upgrades that affect this stat from the pool 
			posUpgrades = posUpgrades.Where(u => u.statID != newPos.statID).ToArray();
			negUpgrades = negUpgrades.Where(u => u.statID != newPos.statID).ToArray();
		}
		// Have 0-2 negative upgrades
		int numNeg = GD.RandRange(0, 2);
		for (int i = 0; i < numNeg; i++)
		{
			PlayerUpgrade newNeg = negUpgrades[GD.RandRange(0, negUpgrades.Length - 1)];
			AddIcon(newNeg);
			int strength = GD.RandRange(0, 2);
			upgradeMagnitudes.Add(newNeg, CalculateMagnitude(newNeg.intChange, strength));
			cost -= strength * 3;
			posUpgrades = posUpgrades.Where(u => u.statID != newNeg.statID).ToArray();
			negUpgrades = negUpgrades.Where(u => u.statID != newNeg.statID).ToArray();
		}
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
				backgroundColour = new Color(0.7f, 0.7f, 1);
				break;
			case 2:
				backgroundColour = new Color(0.8f, 0.5f, 0.3f);
				break;
			case 3:
				backgroundColour = new Color(1, 0, 1);

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
			if (upgrade.intChange)
			{
				float preview = Stats.PlayerStats.DynamicStats[upgrade.statID] + mag * (upgrade.increase ? 1 : -1);;
				infoMessage += string.Format("{0} by {1:D} ({2:D}), ", upgrade.Name, (int)Math.Round(mag), (int)preview);
			}
			else
			{
				float preview = Stats.PlayerStats.DynamicStats[upgrade.statID] + mag * Stats.PlayerStats.BaseStats[upgrade.statID] * (upgrade.increase ? 1 : -1);
				infoMessage += string.Format("{0} by {1:D}% ({2:n2}), ", upgrade.Name, (int)Math.Round(mag * 100), preview);

			}
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
			return (0.2f + GD.Randf() / 5) * (1 + strength);
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
