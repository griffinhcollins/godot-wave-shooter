using Godot;
using System;

public partial class DamageNum : Label
{

	double timeAlive;

	double lifetime = 0.5f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timeAlive = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{

		Position += (float)delta * new Vector2(0, -20);


		timeAlive += delta;
		if (timeAlive >= lifetime)
		{
			QueueFree();
		}
	}
}
