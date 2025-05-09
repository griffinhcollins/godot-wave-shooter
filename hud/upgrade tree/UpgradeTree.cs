using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class UpgradeTree : Control
{

	[Export]
	PackedScene treeNode;

	HBoxContainer container;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		container = GetNode<HBoxContainer>("Container");

		SpawnPrereqs(Stats.PlayerStats.Unlocks.Laser);
	}

	void PrintPrereqs(Prerequisite prereq)
	{
		List<Prerequisite> prereqs = prereq.GetPrerequisites();
		List<string> s;
		if (prereqs is not null && prereqs.Count > 0)
		{
			s = prereqs.Select(u => (u is null) ? "" : u.GetName()).ToList();

		}
		else
		{
			return;
		}
		foreach (string str in s)
		{
			GD.Print(str);
		}
	}

	void SpawnPrereqs(Prerequisite prereq)
	{
		List<Prerequisite> prereqs = prereq.GetPrerequisites();
		foreach (Prerequisite p in prereqs)
		{

			container.AddChild(SpawnNode(p));
		}
	}

	Node SpawnNode(Prerequisite p)
	{
		Node newNode = treeNode.Instantiate();
		newNode.GetNode<TextureRect>("Icon").Texture = (Texture2D)GD.Load("res://custom assets/upgrade icons/" + p.GetIconName());
		newNode.GetNode("Icon").GetNode<Label>("Name").Text = p.GetName();
		return newNode;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
