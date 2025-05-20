using Godot;
using static Stats;
using System.Linq;
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
            if (!PlayerStats.Unlocks.BouncingBullets.unlocked)
            {
                // Turn grey when piercing is done
                GetParent().GetNode<Sprite2D>("Sprite2D").Modulate = Color.Color8(0, 0, 0);
            }
            else
            {
                // If bouncy bullets have been unlocked, become a bouncy bullet
                RigidBody2D newBullet = bounceBullet.Instantiate<RigidBody2D>();
                GetParent().GetParent().AddChild(newBullet);
                BouncyBullet script = newBullet.GetNode<BouncyBullet>("ScriptHolder");
                script.SetVelocity(velocity);
                foreach (Mutation m in currentMutations)
                {
                    script.AddMutation(m);
                    script.SetSeed(seed);
                }
                newBullet.GlobalPosition = GlobalPosition + velocity.Normalized() * 10;
                HandleDeath(null, false);
            }

        }
    }


    protected override void HandleCollision(Node2D hitNode)
    {
        base.HandleCollision(hitNode);
        return;
    }

    public override Vector2 GetCurrentVelocity()
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

    public override void SetVelocity(Vector2 newVelocity, bool normalize = true)
    {
        velocity = newVelocity;
    }

}