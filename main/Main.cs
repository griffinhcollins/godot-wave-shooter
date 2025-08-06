using Godot;
using static Stats;
using System;
using System.Diagnostics.Metrics;
using System.Collections.Generic;
using System.Linq;

public partial class Main : Node2D
{

    [Export]
    public PackedScene[] DefaultMobs { get; set; }
    [Export]
    public PackedScene[] Minibosses { get; set; }

    Mob[] mobLookup;

    List<PackedScene> spawnableMobs;

    private int timeRemaining;
    Player player;
    Marker2D startPos;

    Timer startTimer;
    Timer waveTimer;
    Timer mobTimer;
    Hud hud;

    int mobsSpawned;


    public override void _Ready()
    {
        State.currentState = State.mainmenu;
        player = GetNode<Player>("Player");
        startPos = GetNode<Marker2D>("StartPosition");
        startTimer = GetNode<Timer>("StartTimer");
        waveTimer = GetNode<Timer>("WaveTimer");
        mobTimer = GetNode<Timer>("MobTimer");
        hud = GetNode<Hud>("HUD");
        InitializeMobDictionary();

        GetViewport().GetWindow().FocusExited += OnLoseFocus;

        hud.ShowWave();
        // NewGame();
    }

    // Keep one instance of each mob in memory so we can access the attributes of mob types and associate them with their corresponding index in the packedscene array
    void InitializeMobDictionary()
    {
        mobLookup = new Mob[DefaultMobs.Length];
        for (int i = 0; i < DefaultMobs.Length; i++)
        {
            mobLookup[i] = DefaultMobs[i].Instantiate<Mob>();
        }
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


        if (Input.IsActionJustPressed("pause")) // hit esc
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
        if (State.currentState == State.paused || State.currentState == State.mainmenu)
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
        ResetStats();
        UpdateEnemyStats();
        ClearScreen();
        UnPause();
        player.UpdateStats();
        hud.UpdateMoneyCounter(PlayerStats.Money);
        hud.ClearGameOver();
        StartWave();
    }


    public void UpdateEnemyStats()
    {

        mobTimer.WaitTime = 1 / EnemyStats.DynamicStats[EnemyStats.ID.SpawnRate];

    }

    public void StartWave()
    {
        // Stats.PlayerStats.Unlocks.Shield.Unlock();
        Stats.PlayerStats.Mutations.GrowingBullet.applied = true;
        State.currentState = State.alive;
        UpdateEnemyStats();
        ClearScreen();

        spawnableMobs = DefaultMobs.Where((_, i) => mobLookup[i].firstAppearsAtWave <= Counters.WaveCounter.Value).ToList();


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

        mobsSpawned = 0;
    }

    private void OnWaveTimeout()
    {
        // if (Counters.IsUnlockWave())
        // {
        //     // The round ends once the enemy is defeated
        //     return;
        // }
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


        PathFollow2D mobSpawnLocation = GetNode<PathFollow2D>("MobPath/MobSpawnLocation");

        mobSpawnLocation.ProgressRatio = GD.Randf();

        float direction = mobSpawnLocation.Rotation + Mathf.Pi / 2;
        Vector2 spawnPos = mobSpawnLocation.Position;

        Mob mob;
        if (BossWave())
        {
            // if (mobsSpawned < Mathf.Ceil((Counters.WaveCounter.Value + 1) / 5f))
            // {
            mob = Minibosses[GD.RandRange(0, Minibosses.Count() - 1)].Instantiate<Mob>();

            // }
            // else
            // {
            //     return;
            // }
        }
        else
        {
            



            mob = spawnableMobs[GD.RandRange(0, spawnableMobs.Count - 1)].Instantiate<Mob>();
        }


        if (mob is Drifter) // drifters have a high starting velocity, so give the player a bit more time
        {
            spawnPos -= Vector2.FromAngle(direction) * 400f;
        }
        mob.Position = spawnPos;
        direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
        mob.Rotation = direction;
        mob.Position -= Vector2.FromAngle(direction) * 200f * EnemyStats.DynamicStats[EnemyStats.ID.SizeMult];



        // Vector2 velocity = new Vector2((float)GD.RandRange(150f, 250f), 0);
        // mob.LinearVelocity = velocity.Rotated(direction);

        // The mob doesn't actually exist yet? wack
        AddChild(mob);
        mobsSpawned++;
    }

    bool BossWave()
    {
        return false;
    }


}
