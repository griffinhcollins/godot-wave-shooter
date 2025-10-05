using Godot;
using System;

public partial class Border : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		State.border = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public int CheckBounds(Vector2 pos)
	{
		Vector2 bounds = GetViewportRect().Size;

		if (pos.Y > bounds.Y)
		{
			return 1;
		}
		if (pos.Y < 0)
		{
			return 3;
		}
		if (pos.X < 0)
		{
			return 2;
		}
		if (pos.X > bounds.X)
		{
			return 4;
		}
		return -1;
	}

}
