using Godot;
using System;
using static Stats.PlayerStats.Unlocks;
public partial class FireTrail : Area2D
{

	float timeAlive;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		timeAlive = 0;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (State.currentState == State.paused)
		{
			return;
		}
		timeAlive += (float)delta;
		if (timeAlive > trailLifetime.GetDynamicVal())
		{
			QueueFree();
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Mob)
		{
			((Mob)body).TakeDamage(trailDamage.GetDynamicVal(), DamageTypes.Fire);
			QueueFree();
		}
	}





}
