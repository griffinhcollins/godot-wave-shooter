using Godot;
using System;

public partial class Main : Node
{

    [Export]
    public PackedScene MobScene { get; set; }

    private int score;
    Player player;
    Marker2D startPos;

    Timer startTimer;
    Timer scoreTimer;
    Timer mobTimer;
    Hud hud;

    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        startPos = GetNode<Marker2D>("StartPosition");
        startTimer = GetNode<Timer>("StartTimer");
        scoreTimer = GetNode<Timer>("ScoreTimer");
        mobTimer = GetNode<Timer>("MobTimer");
        hud = GetNode<Hud>("HUD");

        // NewGame();
    }

    public void GameOver()
    {
        hud.ShowGameOver();
        mobTimer.Stop();
        scoreTimer.Stop();
    }

    private void ClearScreen()
    {
        GetTree().CallGroup("mobs", Node.MethodName.QueueFree);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("restart"))
        {
            NewGame();
        }
    }

    public void NewGame()
    {
        ClearScreen();
        score = 0;
        hud.UpdateScore(score);
        hud.ShowMessage("Get Ready!");
        player.Start(startPos.Position);

        startTimer.Start();
        mobTimer.Start();
        scoreTimer.Start();
    }

    private void OnScoreTimeout()
    {
        score++;
        hud.UpdateScore(score);
    }

    private void OnStartTimerTimeout()
    {
        mobTimer.Start();
        scoreTimer.Start();
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
        mob.Position -= Vector2.FromAngle(direction) * 200f;

        Vector2 velocity = new Vector2((float)GD.RandRange(150f, 250f), 0);
        mob.LinearVelocity = velocity.Rotated(direction);

        // The mob doesn't actually exist yet? wack
        AddChild(mob);
    }


}
