using Godot;
using static Stats;
using System.Linq;
using System;
using System.Collections.Generic;

public partial class PiercingBullet : Bullet
{

    [Export]
    PackedScene bounceBullet;

    public Node2D parent;
    public Vector2 velocity;


    public override void _Ready()
    {
        base._Ready();
        parent = GetParent<Node2D>();
    }

    public override void _Process(double delta)
    {
        if (parent is null)
        {
            parent = GetParent<Node2D>();

        }
        base._Process(delta);
        if (State.currentState == State.paused)
        {
            return;
        }
        parent.Position += velocity * (float)delta;
        parent.Rotation = Vector2.Up.AngleTo(velocity);

        if (!dead && timeAlive > PlayerStats.Unlocks.piercingBulletsPiercingTime.GetDynamicVal())
        {
            if (!PlayerStats.Unlocks.BouncingBullets.unlocked)
            {
                // Turn grey when piercing is done
                GetNode<Sprite2D>("MainSprite").Modulate = Color.Color8(0, 0, 0);
            }
            else
            {
                // If bouncy bullets have been unlocked, become a bouncy bullet
                RigidBody2D newBullet = bounceBullet.Instantiate<RigidBody2D>();
                GetParent().GetParent().AddChild(newBullet);
                BouncyBullet script = newBullet.GetNode<BouncyBullet>("ScriptHolder");
                script.SetVelocity(velocity);
                script.SetSeed(seed);
                foreach (Mob mob in mobsHit)
                {
                    script.AddToHitMobs(mob);

                }
                foreach (Mutation m in GetMutations())
                {
                    script.AddMutation(m);
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
        base.SetVelocity(newVelocity);
        if (normalize)
        {
            newVelocity = newVelocity.Normalized() * PlayerStats.ShotSpeed.GetDynamicVal() * shotSpeedMult;
        }
        velocity = newVelocity;
    }

}