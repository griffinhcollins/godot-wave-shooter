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



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		container = GetNode<HBoxContainer>("Container");

		List<Prerequisite> prereqs = Unlocks.Laser.GetPrerequisites();

		UpgradeTreeNode rb1 = SpawnNode(prereqs[0]);
		UpgradeTreeNode rb2 = SpawnNode(prereqs[1]);
		UpgradeTreeNode rb3 = SpawnNode(prereqs[2]);
		rb1.GlobalPosition = new Vector2(-100, 100);
		rb2.GlobalPosition = new Vector2(100, -100);
		rb3.GlobalPosition = new Vector2(100, 100);
		
		AddChild(rb1);
		AddChild(rb2);
		AddChild(rb3);
		ConnectNodes(rb1, rb2);
		ConnectNodes(rb2, rb3);
		ConnectNodes(rb1, rb3);
		



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

	// Parent points to child
	void ConnectNodes(UpgradeTreeNode parent, UpgradeTreeNode child)
	{
		
		DampedSpringJoint2D spring = new DampedSpringJoint2D();
		spring.GlobalPosition = parent.GlobalPosition;
		Vector2 towardsChild = parent.Position - child.Position;
		spring.Length = towardsChild.Length();
		spring.RestLength = 300;
		spring.Damping = 0.5f;
		spring.Rotation = Vector2.Up.AngleTo(towardsChild);
		AddChild(spring);
		spring.NodeA = parent.GetPath();
		spring.NodeB = child.GetPath();
		parent.GlobalRotation = 0;
		child.GlobalRotation = 0;
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

		// Make it look right
		Node visuals = newNode.GetNode("Control");
		visuals.GetNode<TextureRect>("Icon").Texture = (Texture2D)GD.Load("res://custom assets/upgrade icons/" + p.GetIconName());
		visuals.GetNode("Icon").GetNode<Label>("Name").Text = p.GetName();
		return newNode;
	}


	
	List<RigidBody2D> SpawnPrereqs(Prerequisite prereq)
	{
		List<Prerequisite> prereqs = prereq.GetPrerequisites();
		List<RigidBody2D> rigidbodies = new List<RigidBody2D>();
		foreach (Prerequisite p in prereqs)
		{

			RigidBody2D newNode = SpawnNode(p);
			AddChild(newNode);
			newNode.Position = new Vector2(GD.Randf() * 400 - 200, GD.Randf() * 400 - 200);
			rigidbodies.Add(newNode);
		}
		return rigidbodies;
	}

	
}
