using Godot;
using static Stats;
using System;
using System.Collections.Generic;

public partial class PiercingBullet : Bullet
{

    [Export]
    PackedScene bounceBullet;

    public Area2D parent;
    public Vector2 velocity;


    public override void _Ready()
    {
        base._Ready();
        parent = GetParent<Area2D>();
    }

    public override void _Process(double delta)
    {
        if (parent is null)
        {
            parent = GetParent<Area2D>();

        }
        base._Process(delta);
        if (State.currentState == State.paused)
        {
            return;
        }
        parent.Position += velocity * (float)delta;

        if (!dead && timeAlive > PlayerStats.Unlocks.piercingBulletsPiercingTime.GetDynamicVal())
        {
            if (PlayerStats.Unlocks.BouncingBullets.unlocked)
            {
                // Turn grey when piercing is done
                GetParent().GetNode<Sprite2D>("Sprite2D").Modulate = Color.Color8(0, 0, 0);
            }
            else
            {
                Player player = GetParent().GetParent<Player>();
                // If bouncy bullets have been unlocked, become a bouncy bullet
                RigidBody2D newBullet = bounceBullet.Instantiate<RigidBody2D>();
                player.AddChild(newBullet);
                newBullet.LinearVelocity = velocity;
                newBullet.GlobalPosition = GlobalPosition;
                HandleDeath();
            }

        }
    }


    protected override void HandleCollision(Node2D hitNode)
    {
        base.HandleCollision(hitNode);
        return;
    }

    protected override Vector2 GetCurrentVelocity()
    {
        return velocity;
    }

    protected override void Pause()
    {
        return;
    }

    protected override void UnPause()
    {
        return;
    }
}