using Godot;
using System;

public partial class PackedSceneHolder : Node
{

	[Export]
	public PackedScene lightningArc;
	[Export]
	public PackedScene plagueCloud;
	[Export]
	public PackedScene coin;





	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		State.sceneHolder = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
