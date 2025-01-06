using Godot;
using System;

public partial class Main : Node
{

	[Export]
	public PackedScene MobScene { get; set; }

	private int score;


	public override void _Ready()
	{
		NewGame();
	}

	public void GameOver()
	{
		GetNode<Timer>("MobTimer").Stop();
		GetNode<Timer>("ScoreTimer").Stop();
	}


	public void NewGame()
	{
		score = 0;

		Player player = GetNode<Player>("Player");
		Marker2D startPos = GetNode<Marker2D>("StartPosition");
		player.Start(startPos.Position);

		GetNode<Timer>("StartTimer").Start();
	}

	private void OnScoreTimeout()
	{
		score++;
	}

	private void OnStartTimerTimeout()
	{
		GetNode<Timer>("MobTimer").Start();
		GetNode<Timer>("ScoreTimer").Start();
	}


	private void OnMobTimerTimeout()
	{

		Mob mob = MobScene.Instantiate<Mob>();

		PathFollow2D mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");

		mobSpawnLocation.ProgressRatio = GD.Randf();

		float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;

		mob.Position = mobSpawnLocation.Position;
		direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
		mob.Rotation = direction;

		Vector2 velocity = new Vector2((float)GD.RandRange(150f, 250f), 0);
		mob.LinearVelocity = velocity.Rotated(direction);

		// The mob doesn't actually exist yet? wack
		AddChild(mob);
	}


}
