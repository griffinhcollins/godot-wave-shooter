using Godot;
using static Stats.PlayerStats;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public abstract partial class Bullet : Node2D
{

    [Export]
    PackedScene lightningArc;

    protected bool dead;
    float dmg;

    protected float timeAlive;

    // Whether the firing sound has finished sounding - can't delete until it has
    bool soundFinished = false;

    protected Vector2 beforePauseVelocity;

    // A bullet can only hit the same mob once
    HashSet<Node2D> mobsHit;

    int numHit;

    // true only if this was sent by the player (not by another bullet)
    public bool originalBullet;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        dead = false;
        SetDamage(Damage.GetDynamicVal());
        numHit = 0;
        mobsHit = new HashSet<Node2D>();

        if (this is not LaserBeam)
        {
            GetParent().GetNode<CollisionShape2D>("CollisionShape2D").Scale = Vector2.One * BulletSize.GetDynamicVal();
            GetParent().GetNode<Sprite2D>("Sprite2D").Scale = Vector2.One * BulletSize.GetDynamicVal();
        }

        if (originalBullet)
        {
            GetParent().GetNode<AudioStreamPlayer>("FireSound").Play();

        }
        timeAlive = 0;
        CollisionObject2D parent = GetParent<CollisionObject2D>();

        if (Unlocks.WallBounce.unlocked)
        {
            parent.SetCollisionMaskValue(5, true);

        }
    }

    protected abstract Vector2 GetCurrentVelocity();
    protected abstract void Pause();
    protected abstract void UnPause();

    public override void _Process(double delta)
    {
        if (State.currentState == State.paused)
        {
            if (beforePauseVelocity == Vector2.Zero)
            {
                beforePauseVelocity = GetCurrentVelocity();
                Pause();
            }
            return;
        }
        if (beforePauseVelocity != Vector2.Zero)
        {
            UnPause();
            beforePauseVelocity = Vector2.Zero;

        }
        
        timeAlive += (float)delta;
    }


    // The bullet's damage can change after certain events
    public void SetDamage(float newDmg)
    {

        dmg = newDmg;
    }

    private void OnCollision(Node2D body)
    {

        // This always happens when we hit something, regardless of if it is the final hit
        HandleCollision(body);
        //  Now do checks for things that are only on final or non-final hit
        if (
            ((numHit < Unlocks.bouncingBulletBounces.GetDynamicVal()) || // Don't die if we have bounces left
            (timeAlive < Unlocks.piercingBulletsPiercingTime.GetDynamicVal()) || // Don't die if we have piercing left
            Unlocks.OverflowBullets.unlocked) && // Don't die if overflow bullets are unlocked
            dmg > 0 // Do die if we've run out of damage, even if we have bounces/pierces left
         )
        {
            // Not the final hit - do stuff that only triggers on intermediate hits
        }
        else
        {
            // Final hit
            HandleDeath();

        }
    }

    protected virtual void HandleCollision(Node2D hitNode)
    {

        if (hitNode.IsInGroup("border"))
        {
            numHit++;
            // If a piercing shot hits a wall while wall bounces are unlocked, instantly convert it into bouncy
            if (Unlocks.piercingBulletsPiercingTime.GetDynamicVal() > 0)
            {
                timeAlive = Unlocks.piercingBulletsPiercingTime.GetDynamicVal();
            }
            // Reduce damage of bullet when it hits a border
            SetDamage(dmg * Unlocks.wallBounceDamageRetention.GetDynamicVal());
        }
        // Activate any abilities that trigger on hit
        if (hitNode is Mob)
        {
            Mob hitMob = (Mob)hitNode;
            if (Unlocks.Lightning.unlocked)

            {
                // Find the nearest mob
                Mob target = null;
                foreach (Mob mob in GetTree().GetNodesInGroup("mobs"))
                {
                    if (mob == hitNode)
                    {
                        continue;
                    }
                    if (target is null) // just to give an initial to start testing against
                    {
                        target = mob;
                    }
                    float currentBestDistance = (target.GlobalPosition - GlobalPosition).LengthSquared();
                    if ((mob.GlobalPosition - GlobalPosition).LengthSquared() < currentBestDistance)
                    {
                        target = mob;
                    }
                }

                // Contingency for if there are no mobs
                if (target is null)
                {
                    return;
                }


                float arcRange = Unlocks.lightningRange.GetDynamicVal();
                if ((target.GlobalPosition - GlobalPosition).LengthSquared() < arcRange * arcRange)
                {
                    Node2D arc = lightningArc.Instantiate<Node2D>();
                    Line2D line = arc.GetNode<Line2D>("Arc");
                    GetTree().Root.AddChild(arc);
                    line.AddPoint(hitMob.GlobalPosition);
                    line.AddPoint(target.GlobalPosition);

                    target.TakeDamage(Damage.GetDynamicVal());
                }
            }
            if (Unlocks.OverflowBullets.unlocked)
            {
                SetDamage(dmg - Unlocks.overflowLoss.GetDynamicVal() * hitMob.GetHP());
            }
        }
        // Finally, actually deal damage
        if (hitNode.IsInGroup("mobs") && !mobsHit.Contains(hitNode))
        {
            numHit++;
            mobsHit.Add(hitNode);
            Mob mobHit = (Mob)hitNode;
            mobHit.TakeDamage(dmg);
        }

    }
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
