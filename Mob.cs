using System;
using Godot;

public partial class Mob : RigidBody2D
{

    Player player;

    float baseHealth = 2;
    float hp;
    float baseSpeedLimit = 500;
    float speedLimit;


    [Export]
    public PackedScene coin;

    float healthMult;
    float speedMult;
    float dropRate; // How likely an enemy is to drop money. If above 100, enemy can drop more than 1 coin



    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UpdateStats();
        hp = baseHealth * healthMult;
        player = (Player)GetTree().GetNodesInGroup("player")[0];
        speedLimit = baseSpeedLimit * speedMult;
        AnimatedSprite2D animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        string[] mobTypes = animSprite.SpriteFrames.GetAnimationNames();
        animSprite.Play(mobTypes[GD.Randi() % mobTypes.Length]);
    }

    private void UpdateStats()
    {
        healthMult = Stats.Enemy.HealthMult;
        speedMult = Stats.Enemy.SpeedMult;
        dropRate = Stats.Enemy.DropRate;
    }


    public void TakeDamage(float dmg)
    {
        hp -= dmg;
        if (hp <= 0)
        {
            Die();
        }
    }


    private void Die()
    {
        float tempDropRate = dropRate;
        // If drop rate is above 1, get 1 guaranteed coin plus a chance at another
        while (tempDropRate > 0)
        {
            if (GD.RandRange(0f, 1) <= tempDropRate)
            {
                SpawnCoin();
            }
            tempDropRate--;
        }
        QueueFree();

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
        ApplyForce(player.Position - Position);
        ApplyTorque(LinearVelocity.AngleTo(ToGlobal(Vector2.Up)) * 1000);
        float currentSpeedSq = LinearVelocity.LengthSquared();
        if (currentSpeedSq > speedLimit * speedLimit)
        {
            LinearVelocity = LinearVelocity.Normalized() * speedLimit;
            // ApplyForce(LinearVelocity * (speedLimit - Mathf.Pow(currentSpeedSq, 0.5f)));
        }


    }

    private void OnScreenExit()
    {
        //QueueFree();
    }
}
