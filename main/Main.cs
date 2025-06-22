using Godot;
using static Stats;
using System;
using System.Diagnostics.Metrics;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node2D
{

    [Export]
    public PackedScene[] MobScenes { get; set; }

    private int timeRemaining;
    Player player;
    Marker2D startPos;

    Timer startTimer;
    Timer waveTimer;
    Timer mobTimer;
    Hud hud;




    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        startPos = GetNode<Marker2D>("StartPosition");
        startTimer = GetNode<Timer>("StartTimer");
        waveTimer = GetNode<Timer>("WaveTimer");
        mobTimer = GetNode<Timer>("MobTimer");
        hud = GetNode<Hud>("HUD");

        GetViewport().GetWindow().FocusExited += OnLoseFocus;

        hud.ShowWave();
        // NewGame();
    }




    public void GameOver()
    {

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

    private void OnLoseFocus()
    {
        Pause();
    }

    private void TogglePause()
    {

        if (State.currentState != State.paused)
        {
            Pause();

        }
        else
        {
            UnPause();
        }
    }

    public void Pause()
    {
        if (State.currentState == State.paused)
        {
            return;
        }
        hud.ShowPauseMenu();
        State.unPauseState = State.currentState;
        State.currentState = State.paused;
        startTimer.Paused = true;
        waveTimer.Paused = true;
        mobTimer.Paused = true;
    }
    public void UnPause()
    {
        hud.HidePauseMenu();
        State.currentState = State.unPauseState;
        startTimer.Paused = false;
        waveTimer.Paused = false;
        mobTimer.Paused = false;
    }

    private void NewGame()
    {
        GetNode<Label>("Title").Hide();
        ResetStats();
        UpdateEnemyStats();
        ClearScreen();
        UnPause();
        player.UpdateStats();
        hud.UpdateMoneyCounter(PlayerStats.Money);
        hud.ClearGameOver();
        StartWave();
        Stats.PlayerStats.Unlocks.DeathExplosion.Unlock();
        Stats.PlayerStats.Unlocks.Lightning.Unlock();
        PlayerStats.Unlocks.Laser.Unlock();
        // Stats.PlayerStats.Unlocks.Splinter.Unlock();
        // Stats.PlayerStats.Unlocks.Plague.Unlock();

    }


    public void UpdateEnemyStats()
    {

        mobTimer.WaitTime = 1 / EnemyStats.DynamicStats[EnemyStats.ID.SpawnRate];

    }

    public void StartWave()
    {
        // Stats.PlayerStats.Mutations.DeceleratingBullet.applied = true;
        State.currentState = State.alive;
        UpdateEnemyStats();
        ClearScreen();
        timeRemaining = (int)EnemyStats.DynamicStats[EnemyStats.ID.WaveLength];
        hud.UpdateWaveTime(timeRemaining);
        string waveUpdate = string.Format("Starting Wave {0}", Counters.WaveCounter.Value);
        if (EnemyStats.mostRecentMutation is not null && Counters.WaveCounter.Value % 3 == 0)
        {
            waveUpdate += string.Format("\n{0}", EnemyStats.mostRecentMutation);
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

        waveTimer.Stop();
        mobTimer.Stop();
        ClearScreen();
        hud.ShowMessage(string.Format("Wave {0} Complete!", Counters.WaveCounter.Value));
        Counters.WaveCounter.Value++;
        if (Counters.WaveCounter.Value % 3 == 0)
        {
            EnemyStats.IncreaseDifficulty();

        }

        player.AddMoney((int)PlayerStats.HPReward.GetDynamicVal() * player.CurrentHP());

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

        Mob mob = MobScenes[GD.RandRange(0, MobScenes.Count() - 1)].Instantiate<Mob>();

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
