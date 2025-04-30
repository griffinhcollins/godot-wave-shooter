using System;
using static Stats.EnemyStats;
using Godot;

public partial class Mob : RigidBody2D
{

    Player player;

    float baseHealth = 20;
    float baseSpeedLimit = 400;
    float baseAcceleration = 1;
    float hp;
    float speedLimit;
    float acceleration;

    bool dead = false;

    [Export]
    PackedScene offscreenIndicator;


    OffscreenIndicator pairedIndicator;

    AudioStreamPlayer2D damageSound;

    [Export]
    public PackedScene coin;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        hp = DynamicStats[ID.HPMult] * baseHealth;
        acceleration = baseAcceleration * DynamicStats[ID.AccelerationMult];
        player = (Player)GetTree().GetNodesInGroup("player")[0];
        speedLimit = baseSpeedLimit;
        AnimatedSprite2D animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        string[] mobTypes = animSprite.SpriteFrames.GetAnimationNames();
        animSprite.Play(mobTypes[GD.Randi() % mobTypes.Length]);
        damageSound = GetNode<AudioStreamPlayer2D>("DamageSound");
        CreateIndicator();
    }



    public void TakeDamage(float dmg)
    {
        Hud hud = GetParent().GetNode<Hud>("HUD");
        hud.CreateDamageNumber(Position, dmg);
        damageSound.Play();
        hp -= dmg;
        if (hp <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        dead = true;
        float tempDropRate = Stats.PlayerStats.DropRate.GetDynamicVal();
        // If drop rate is above 1, get 1 guaranteed coin plus a chance at another
        while (tempDropRate > 0)
        {
            if (GD.RandRange(0f, 1) <= tempDropRate)
            {
                SpawnCoin();
            }
            tempDropRate--;
        }
        // Don't queuefree yet, that happens once the damage sound is complete
        Hide();
        CollisionLayer = 0;
        CollisionMask = 0;


    }

    private void SpawnCoin()
    {
        Coin newCoin = coin.Instantiate<Coin>();
        newCoin.Position = Position + new Vector2(GD.Randf() * 2 - 1, GD.Randf() * 2 - 1) * 20;
        GetParent().CallDeferred("add_child", newCoin);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Point towards the player
        ApplyForce((player.Position - Position) * ((player.Position - Position).Length() * 1 / 1000 + speedLimit / 500) * acceleration);
        ApplyTorque(LinearVelocity.AngleTo(ToGlobal(Vector2.Up)) * 1000);
        float currentSpeedSq = LinearVelocity.LengthSquared();
        if (currentSpeedSq > speedLimit * speedLimit)
        {
            LinearVelocity = LinearVelocity.Normalized() * speedLimit;
            // ApplyForce(LinearVelocity * (speedLimit - Mathf.Pow(currentSpeedSq, 0.5f)));
        }


    }

    private void CreateIndicator()
    {
        pairedIndicator = offscreenIndicator.Instantiate<OffscreenIndicator>();
        pairedIndicator.SetMobParent(this);
        GetTree().Root.AddChild(pairedIndicator);
        pairedIndicator.Position = Position;
    }

    private void OnScreenExit()
    {
        if (!dead)
        {
            CreateIndicator();

        }
    }

    private void OnScreenEnter()
    {
        if (pairedIndicator is not null)
        {
            pairedIndicator.QueueFree();
        }
    }


    private void OnDamageSoundFinished()
    {
        if (dead)
        {
            QueueFree();
        }
    }


}
