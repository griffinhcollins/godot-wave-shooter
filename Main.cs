using Godot;
using System;

public partial class Main : Node
{

    [Export]
    public PackedScene MobScene { get; set; }

    private int timeRemaining;
    Player player;
    Marker2D startPos;

    Timer startTimer;
    Timer waveTimer;
    Timer mobTimer;
    Hud hud;

    // 0 = in game, 1 = between waves, 2 = dead
    int state;

    int waveLength;

    float spawnRate; // How many enemies spawn per second

    int waveCounter;

    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        startPos = GetNode<Marker2D>("StartPosition");
        startTimer = GetNode<Timer>("StartTimer");
        waveTimer = GetNode<Timer>("WaveTimer");
        mobTimer = GetNode<Timer>("MobTimer");
        hud = GetNode<Hud>("HUD");

        hud.ShowWave();
        // NewGame();
    }

    public void GameOver()
    {
        Stats.Reset();
        
        state = State.dead;
        hud.ShowGameOver();
        mobTimer.Stop();
        waveTimer.Stop();

    }

    private void ClearScreen()
    {
        GetTree().CallGroup("cleanup", Node.MethodName.QueueFree);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("restart") && (state == State.dead))
        {
            StartWave();
        }
    }

    private void NewGame()
    {
        Stats.Reset();
        ClearScreen();
        player.UpdateStats();
        UpdateEnemyStats();
        waveCounter = 1;
        state = State.alive;
        StartWave();
    }


    public void UpdateEnemyStats()
    {

        waveLength = Stats.Enemy.WaveLength;
        spawnRate = Stats.Enemy.SpawnRate;
        mobTimer.WaitTime = 1 / spawnRate;

    }

    public void StartWave()
    {
        ClearScreen();
        timeRemaining = waveLength;
        hud.UpdateWaveTime(timeRemaining);
        string waveUpdate = string.Format("Starting Wave {0}", waveCounter);
        if (Stats.Enemy.LastMutation is not null){
            waveUpdate += string.Format("\n{0}", Stats.Enemy.LastMutation);
        }
        hud.ShowMessage(waveUpdate);
        player.Start(startPos.Position);

        startTimer.Start();
        mobTimer.Start();
        waveTimer.Start();
    }

    private void OnWaveTimeout()
    {
        timeRemaining--;
        hud.UpdateWaveTime(timeRemaining);
        if (timeRemaining == 0)
        {
            EndWave();
        }
    }

    private void EndWave()
    {
        Stats.Enemy.IncreaseRandomStats();
        waveTimer.Stop();
        mobTimer.Stop();
        ClearScreen();
        hud.ShowMessage(string.Format("Wave {0} Complete!", waveCounter));
        waveCounter++;

        player.AddMoney(Stats.Player.BaseHPReward * player.CurrentHP());


        state = State.shop;
        hud.GenerateShop();
    }


    private void OnStartTimerTimeout()
    {
        mobTimer.Start();
        waveTimer.Start();
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

        // Vector2 velocity = new Vector2((float)GD.RandRange(150f, 250f), 0);
        // mob.LinearVelocity = velocity.Rotated(direction);

        // The mob doesn't actually exist yet? wack
        AddChild(mob);
    }


}
