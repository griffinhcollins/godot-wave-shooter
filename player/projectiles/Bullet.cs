using Godot;
using static Stats.PlayerStats;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public abstract partial class Bullet : Node2D
{
    protected bool dead;
    float dmg;

    protected float timeAlive;

    // Whether the firing sound has finished sounding - can't delete until it has
    bool soundFinished = false;

    // A bullet can only hit the same mob once
    HashSet<Node2D> mobsHit;

    int numHit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        dead = false;
        SetDamage(Damage.GetDynamicVal());
        numHit = 0;
        mobsHit = new HashSet<Node2D>();
        GetParent().GetNode<AudioStreamPlayer>("FireSound").Play();
        GD.Print("val");
        GD.Print(Vector2.One * BulletSize.GetDynamicVal());
        timeAlive = 0;
        CollisionObject2D parent = GetParent<CollisionObject2D>();

        if (Unlocks.WallBounce.unlocked)
        {
            parent.SetCollisionMaskValue(5, true);

        }
    }

    public override void _Process(double delta)
    {
        timeAlive += (float)delta;
    }


    public void SetDamage(float newDmg)
    {
        dmg = newDmg;
    }

    private void OnCollision(Node2D body)
    {
        if (body.IsInGroup("mobs") && !mobsHit.Contains(body))
        {
            numHit++;
            mobsHit.Add(body);
            Mob mobHit = (Mob)body;
            mobHit.TakeDamage(dmg);
        }
        if (body.IsInGroup("border"))
        {
            numHit++;
            // If a piercing shot hits a wall while wall bounces are unlocked, instantly convert it into bouncy
            if (Piercing.GetDynamicVal() > 0)
            {
                timeAlive = Piercing.GetDynamicVal();
            }

        }
        if (numHit > Bounces.GetDynamicVal() && timeAlive > Piercing.GetDynamicVal())
        {
            HandleDeath();
        }
        else
        {
            // We hit a mob but we have hits remaining
            // What this means changes depending on what kind of bullet we are
            HandleCollision();
        }
    }

    protected abstract void HandleCollision();
    protected virtual void HandleDeath()
    {
        dead = true;
        Node2D parent = GetParent<Node2D>();
        parent.Hide();
        ((CollisionObject2D)parent).CollisionLayer = 0; // Disable collision
        ((CollisionObject2D)parent).CollisionMask = 0; // Disable collision
        if (soundFinished)
        {
            parent.QueueFree();

        }
    }

    private void SoundFinished()
    {
        soundFinished = true;
        if (dead)
        {
            QueueFree();

        }
    }


}
