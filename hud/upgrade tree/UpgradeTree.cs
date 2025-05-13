using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Stats.PlayerStats;


public partial class UpgradeTree : Node
{

	[Export]
	PackedScene treeNode;

	HBoxContainer container;

	float linkRestDistance = 200;

	UpgradeTreeNode submarine;

	Dictionary<string, UpgradeTreeNode> nameLookup;
	Dictionary<Prerequisite, List<Prerequisite>> childrenLookup; // The value is a list of everything that is (partially or fully) unlocked by the key


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		container = GetNode<HBoxContainer>("Container");

		// Create our root
		submarine = treeNode.Instantiate<UpgradeTreeNode>();
		submarine.root = submarine;
		// Make it look right
		Node visuals = submarine.GetNode("Control");
		visuals.GetNode<TextureRect>("Icon").Texture = (Texture2D)GD.Load("res://custom assets/hud/upgrade background.png");
		visuals.GetNode("Icon").GetNode<Label>("Name").Text = "Root";
		AddChild(submarine);


		List<Prerequisite> allStatsAndUnlocks = GetDefaultStats().ToList<Prerequisite>(); // Just grabs the default stats for now
		foreach (Unlockable u in Unlocks.allUnlockables)
		{
			allStatsAndUnlocks.Add(u);
			foreach (Prerequisite stat in u.associatedStats.upgradeableStats)
			{
				allStatsAndUnlocks.Add(stat);
			}
		}


		// Populate the lookup dictionary
		nameLookup = new Dictionary<string, UpgradeTreeNode>();
		foreach (Prerequisite p in allStatsAndUnlocks)
		{
			UpgradeTreeNode newNode = SpawnNode(p);
			AddChild(newNode);
			nameLookup.Add(p.GetName(), newNode);
		}


		// Establish connections between all the prerequisite links

		// Keep track of the base upgrades for spacing purposes
		List<Prerequisite> baseUpgrades = new List<Prerequisite>();

		childrenLookup = new Dictionary<Prerequisite, List<Prerequisite>>();

		foreach (Prerequisite p in allStatsAndUnlocks)
		{
			UpgradeTreeNode n = nameLookup[p.GetName()];
			List<Prerequisite> prereqs = p.GetPrerequisites();
			if (prereqs is null)
			{
				// Base uppgrade, connect it to the submarine
				baseUpgrades.Add(p);
				ConnectNodes(n, submarine);
				continue;
			}

			foreach (Prerequisite unlocker in prereqs)
			{
				AddRequirement(unlocker, p);
				ConnectNodes(n, nameLookup[unlocker.GetName()]);
			}
		}

		// Spread the upgrades out based on how far they are from the root
		for (int i = 0; i < baseUpgrades.Count; i++)
		{
			Vector2 newPos = new Vector2(0, -linkRestDistance).Rotated(i * 2 * Mathf.Pi / baseUpgrades.Count);
			PositionNodeAndChildren(baseUpgrades[i], newPos);


		}

		


	}

	void PositionNodeAndChildren(Prerequisite root, Vector2 direction)
	{
		UpgradeTreeNode nodeToPosition = nameLookup[root.GetName()];

		nodeToPosition.Position = direction;
		if (!childrenLookup.Keys.Contains(root))
		{
			return;
		}


		// Move the things unlocked by this upgrade out a little more
		foreach (Prerequisite p in childrenLookup[root])
		{
			PositionNodeAndChildren(p, direction + direction.Normalized() * linkRestDistance);
		}
	}

	void AddRequirement(Prerequisite unlocker, Prerequisite unlockee)
	{
		if (!childrenLookup.Keys.Contains(unlocker))
		{
			childrenLookup[unlocker] = new List<Prerequisite> { unlockee };
		}
		else
		{
			childrenLookup[unlocker].Add(unlockee);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
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

	void AddSpring(UpgradeTreeNode node1, UpgradeTreeNode node2, float damping = 1, float distance = -1)
	{
		if (distance == -1)
		{
			distance = linkRestDistance;
		}
		DampedSpringJoint2D spring = new DampedSpringJoint2D();
		spring.GlobalPosition = node1.GlobalPosition;
		Vector2 towardsChild = node1.Position - node2.Position;
		spring.Length = towardsChild.Length();
		spring.RestLength = linkRestDistance;
		spring.Damping = damping;
		spring.Rotation = Vector2.Up.AngleTo(towardsChild);
		AddChild(spring);
		spring.NodeA = node1.GetPath();
		spring.NodeB = node2.GetPath();
		node1.GlobalRotation = 0;
		node2.GlobalRotation = 0;
	}

	// Parent points to child
	void ConnectNodes(UpgradeTreeNode parent, UpgradeTreeNode child)
	{

		// AddSpring(parent, child, 1);


		// Now make the line renderer connecting them

		Line2D connectionLine = new Line2D();
		AddChild(connectionLine);
		connectionLine.AddPoint(parent.Position);
		connectionLine.AddPoint(child.Position);
		parent.AddLine(connectionLine, 0);
		child.AddLine(connectionLine, 1);
		connectionLine.ZIndex = -1;
	}




	UpgradeTreeNode SpawnNode(Prerequisite p)
	{
		UpgradeTreeNode newNode = treeNode.Instantiate<UpgradeTreeNode>();
		newNode.root = submarine;
		// Make it look right
		Node visuals = newNode.GetNode("Control");
		visuals.GetNode<TextureRect>("Icon").Texture = (Texture2D)GD.Load("res://custom assets/upgrade icons/" + p.GetIconName());
		visuals.GetNode("Icon").GetNode<Label>("Name").Text = p.GetName();

		return newNode;
	}



	List<UpgradeTreeNode> SpawnPrereqs(Prerequisite prereq)
	{
		List<Prerequisite> prereqs = prereq.GetPrerequisites();
		List<UpgradeTreeNode> rigidbodies = new List<UpgradeTreeNode>();
		foreach (Prerequisite p in prereqs)
		{

			UpgradeTreeNode newNode = SpawnNode(p);
			AddChild(newNode);
			newNode.Position = new Vector2(GD.Randf() * 400 - 200, GD.Randf() * 400 - 200);
			rigidbodies.Add(newNode);
		}
		return rigidbodies;
	}


}
