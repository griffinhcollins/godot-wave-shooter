using System;
using static Stats.Enemy;
using Godot;

public partial class Mob : RigidBody2D
{

    Player player;

    float baseHealth = 2;
    float baseSpeedLimit = 500;
    float baseAcceleration = 1;
    float hp;
    float speedLimit;
    float acceleration;


    [Export]
    public PackedScene coin;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        hp = HealthMult * baseHealth;
        acceleration = baseAcceleration * AccelerationMult;
        player = (Player)GetTree().GetNodesInGroup("player")[0];
        speedLimit = SpeedMult * baseSpeedLimit;
        AnimatedSprite2D animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        string[] mobTypes = animSprite.SpriteFrames.GetAnimationNames();
        animSprite.Play(mobTypes[GD.Randi() % mobTypes.Length]);
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
        float tempDropRate = DropRate;
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
        ApplyForce((player.Position - Position) * ((player.Position - Position).Length() * 1/1000 + speedLimit/500) * acceleration);
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
