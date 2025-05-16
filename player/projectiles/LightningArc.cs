using Godot;
using System;

public partial class LightningArc : Node2D
{
	[Export]
	Texture2D lightningOne;
	[Export]
	Texture2D lightningTwo;
	[Export]
	Texture2D lightningThree;

	Line2D arcLine;
	double timeAlive;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		arcLine = GetNode<Line2D>("Arc");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// timeAlive += delta;
		// if (timeAlive > 0.03f)
		// {
		// 	arcLine.Texture = lightningTwo;
		// }
		// if (timeAlive > 0.06f)
		// {
		// 	arcLine.Texture = lightningThree;
		// }
	}

	void OnTimerComplete()
	{
		QueueFree();
	}
}
