using Godot;
using System;
using System.Collections.Generic;

public partial class UpgradeTreeNode : RigidBody2D
{
	Dictionary<Line2D, int> lineIndices;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		lineIndices = new Dictionary<Line2D, int>();
	}

	public void AddLine(Line2D line, int index)
	{
		lineIndices.Add(line, index);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		foreach (Line2D line in lineIndices.Keys)
		{
			line.SetPointPosition(lineIndices[line], Position);
			
		}
	}
}
