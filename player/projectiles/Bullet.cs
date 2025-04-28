using Godot;
using static Stats.PlayerStats;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public abstract partial class Bullet : Node2D
{

    float dmg;

    // Whether the firing sound has finished sounding - can't delete until it has
    bool soundFinished = false;

    // A bullet can only hit the same mob once
    HashSet<Node2D> mobsHit;

    int numHit;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetDamage(Damage.GetDynamicVal());
        numHit = 0;
        mobsHit = new HashSet<Node2D>();
        GetParent().GetNode<AudioStreamPlayer>("FireSound").Play();
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

        }
        if (numHit > Mathf.Max(Bounces.GetDynamicVal(), Pierces.GetDynamicVal()))
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
        Node2D parent = GetParent<Node2D>();
        parent.Hide();
        ((CollisionObject2D)parent).CollisionLayer = 0; // Disable collision
        if (soundFinished)
        {
            parent.QueueFree();

        }
    }

    private void SoundFinished()
    {
        soundFinished = true;
        if (GetParent<Node2D>().ProcessMode == ProcessModeEnum.Disabled)
        {
            QueueFree();

        }
    }


}
