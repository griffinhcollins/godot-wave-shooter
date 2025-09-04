using Godot;
using System;

public partial class AudioManager : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		State.audioManager = this;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}


	public void PlaySound(string soundName)
	{
		
	}

}
