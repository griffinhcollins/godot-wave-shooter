using Godot;
using static Stats.Upgrades;
using System;
using System.Collections.Generic;

public partial class Upgrade : Button
{

	HFlowContainer iconHolder;

	Dictionary<int, float> upgradeMagnitudes;

	int cost;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		upgradeMagnitudes = new Dictionary<int, float>();
		iconHolder = GetNode<HFlowContainer>("IconHolder");
		Randomize();
		UpdateCost();
	}

	void Randomize(){

		// Upgrades can have multiple positives and negatives
		// 0: Common: 1 positive, regular range
		// 1: Uncommon: 1 positive, 1 negative, extended for positive, regular for negative
		// 2: Rare: 2 positive, 1 negative, 1 pos is standard, other two are extended
		// 3: Cursed: 2 positive, 2 negative, extreme range for positives, extended range for negatives
		int rarity = 0;
		int rarityRoll = GD.RandRange(0, 20);
		if (rarityRoll >= 10){
			rarity++;
		}
		if (rarityRoll >= 15){
			rarity++;
		}
		if (rarityRoll >= 18){
			rarity++;
		}


		// TODO: Make this better, maybe have real bad upgrades that cost negative money?
		cost = 4*(rarity + 1) + GD.RandRange(-2, 2);
		HashSet<int> pickedStats = new HashSet<int>();

		int pos1 = RandEven(); 
		pickedStats.Add(pos1);
		pickedStats.Add(pos1 + 1);
		int pos2 = RandEven(pickedStats); 
		pickedStats.Add(pos2);
		pickedStats.Add(pos2 + 1);
		int neg1 = RandOdd(pickedStats); 
		pickedStats.Add(neg1);
		pickedStats.Add(neg1 - 1);
		int neg2 = RandOdd(pickedStats);
		// All rarities get pos1
		AddIcon(pos1);
		// Every rarity except cursed contains a standard range positive, rare gets an extreme
		switch(rarity){
			case 0:
			// Common
				upgradeMagnitudes.Add(pos1, CalculateMagnitude(pos1, 0));
				break;
			case 1:
			// Uncommon
				upgradeMagnitudes.Add(pos1, CalculateMagnitude(pos1, 1));
				AddIcon(neg1);
				upgradeMagnitudes.Add(neg1, CalculateMagnitude(neg1, 0));
				break;
			case 2:
			// Rare
				upgradeMagnitudes.Add(pos1, CalculateMagnitude(pos1, 0));
				AddIcon(pos2);
				upgradeMagnitudes.Add(pos2, CalculateMagnitude(pos2, 1));
				AddIcon(neg1);
				upgradeMagnitudes.Add(neg1, CalculateMagnitude(neg1, 1));
				break;
			case 3:
			// Cursed
				upgradeMagnitudes.Add(pos1, CalculateMagnitude(pos1, 2));
				AddIcon(pos2);
				upgradeMagnitudes.Add(pos2, CalculateMagnitude(pos2, 2));
				AddIcon(neg1);
				upgradeMagnitudes.Add(neg1, CalculateMagnitude(neg1, 2));
				AddIcon(neg2);
				upgradeMagnitudes.Add(neg2, CalculateMagnitude(neg2, 2));
				break;


		}

	}

	private void OnClicked(){
		GD.Print("clicked an upgrade!");
		if (!((Player)GetTree().GetNodesInGroup("player")[0]).ChargeMoney(cost)){
			return;
		}

		foreach (int ID in upgradeMagnitudes.Keys)
		{
			ExecuteUpgrade(ID, upgradeMagnitudes[ID]);
		}

		QueueFree();
		
	}

	// strength can be 0 (standard), 1 (extended) or 2+ (extreme)
	private float CalculateMagnitude(int ID, int strength){
		switch(ID){
			// HP
			case 4:
			case 5:
			case 6:
			case 7:
				return Math.Max(strength,1);

			default:
				return (0.2f + GD.Randf() / 5) * (1 + strength);
			
		}
	}

	void UpdateCost(){
		GetNode<Label>("Cost").Text = string.Format("${0}", cost);
	}

	private void AddIcon(int ID){
		TextureRect textureRect = new TextureRect();
		textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		textureRect.CustomMinimumSize = new Vector2(45, 45);
		textureRect.Texture = GetUpgradeIcon(ID);
		iconHolder.AddChild(textureRect);
	}

	int RandEven(HashSet<int> exclude = null){
		if (exclude is null){
			exclude = new HashSet<int>();
		}
		exclude.Add(-1);
		int val = -1;
		while (exclude.Contains(val)){
			val = 2*(GD.RandRange(0,ID.numUpgrades - 1) / 2);
		}
		return val;
	}
	int RandOdd(HashSet<int> exclude = null){
		if (exclude is null){
			exclude = new HashSet<int>();
		}
		exclude.Add(-1);
		int val = -1;
		while (exclude.Contains(val)){
			val = 2*(GD.RandRange(0,ID.numUpgrades - 1) / 2) + 1;
		}
		return val;
	}
}
