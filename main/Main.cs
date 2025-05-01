using Godot;
using static Stats;
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
        ResetStats();

        State.currentState = State.dead;
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
        if (Input.IsActionJustPressed("restart") && (State.currentState == State.dead))
        {
            NewGame();
        }

        if (Input.IsActionJustPressed("pause"))
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (State.currentState != State.paused)
        {
            hud.ShowPauseMenu();
            State.unPauseState = State.currentState;
            State.currentState = State.paused;
            startTimer.Paused = true;
            waveTimer.Paused = true;
            mobTimer.Paused = true;
        }
        else
        {
            hud.HidePauseMenu();
            State.currentState = State.unPauseState;
            startTimer.Paused = false;
            waveTimer.Paused = false;
            mobTimer.Paused = false;
        }
    }

    private void NewGame()
    {
        GetNode<Label>("Title").Hide();
        ResetStats();
        UpdateEnemyStats();
        ClearScreen();
        player.UpdateStats();
        waveCounter = 1;
        hud.UpdateMoneyCounter(PlayerStats.Money);
        StartWave();
    }


    public void UpdateEnemyStats()
    {

        waveLength = (int)EnemyStats.DynamicStats[EnemyStats.ID.WaveLength];
        spawnRate = EnemyStats.DynamicStats[EnemyStats.ID.SpawnRate];
        mobTimer.WaitTime = 1 / spawnRate;

    }

    public void StartWave()
    {
        State.currentState = State.alive;
        UpdateEnemyStats();
        ClearScreen();
        timeRemaining = waveLength;
        hud.UpdateWaveTime(timeRemaining);
        string waveUpdate = string.Format("Starting Wave {0}", waveCounter);
        if (EnemyStats.LastMutation is not null)
        {
            waveUpdate += string.Format("\n{0}", EnemyStats.LastMutation);
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
        EnemyStats.IncreaseDifficulty();
        waveTimer.Stop();
        mobTimer.Stop();
        ClearScreen();
        hud.ShowMessage(string.Format("Wave {0} Complete!", waveCounter));
        waveCounter++;

        player.AddMoney((int)PlayerStats.HPReward.GetDynamicVal() * player.CurrentHP());

        Upgrades.GenerateUpgrades();
        State.currentState = State.shop;
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
        mob.Position -= Vector2.FromAngle(direction) * 200f * EnemyStats.DynamicStats[EnemyStats.ID.SizeMult];



        // Vector2 velocity = new Vector2((float)GD.RandRange(150f, 250f), 0);
        // mob.LinearVelocity = velocity.Rotated(direction);

        // The mob doesn't actually exist yet? wack
        AddChild(mob);
    }


}
