using System;
using static Stats.EnemyStats;
using static Stats.PlayerStats.Unlocks;
using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;

public abstract partial class Mob : RigidBody2D, IAffectedByVisualEffects
{

    protected Player player;

    // Venom
    bool poisoned = false;
    float poisonTick;

    // Explosions!
    protected bool explodeOnDeath = false;

    protected float hp;


    protected Vector2 beforePauseVelocity;
    protected Vector2 beforePausePosition;
    protected float beforePauseAngularVelocity;

    bool dead = false;


    [Export]
    protected PackedScene offscreenIndicator;


    protected float size;

    protected OffscreenIndicator pairedIndicator;

    AudioStreamPlayer2D damageSound;




    protected AnimatedSprite2D animSprite;


    public List<VisualEffect> visualEffects { get; set; }

    public Dictionary<StaticColourChange, float> staticColours { get; set; }
    public Dictionary<ParticleEffect, GpuParticles2D> instantiatedParticles { get; set; } // Particle effects should only instantiate once
    public HashSet<Improvement> overwrittenSources { get; set; }
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {

        player = (Player)GetTree().GetNodesInGroup("player")[0];
        animSprite = GetNode<AnimatedSprite2D>("MainSprite");
        string[] mobTypes = animSprite.SpriteFrames.GetAnimationNames();
        animSprite.Play(mobTypes[GD.Randi() % mobTypes.Length]);
        damageSound = GetNode<AudioStreamPlayer2D>("DamageSound");

        // Set Size
        SetSize();
        SetScale();
        CreateIndicator();

        if (DeathExplosion.unlocked)
        {
            if (GD.Randf() < explosionChance.GetDynamicVal())
            {
                explodeOnDeath = true;
            }
        }




        ((IAffectedByVisualEffects)this).ImmediateVisualEffects();

    }

    protected virtual void SetSize()
    {
        size = GD.Randf() * 0.5f * DynamicStats[ID.SizeMult] + 1;
    }

    protected virtual void SetScale()
    {
        animSprite.SpeedScale = 1 / (size);
        hp = DynamicStats[ID.HPMult] * GetBaseHealth() * size * 0.75f;
        animSprite.Scale *= size;
        GetNode<CollisionShape2D>("Collider").Scale *= size;
    }

    protected virtual float GetBaseHealth()
    {
        return 20;
    }


    // Used when the player is hit to give them some breathing room
    public virtual void Recoil(Vector2 recoilFrom, float mult = 1)
    {
        float recoilRange = 800;
        if ((GlobalPosition - recoilFrom).LengthSquared() < recoilRange * recoilRange)
        {
            float distance = (GlobalPosition - recoilFrom).Length();
            LinearVelocity = Vector2.Zero;
            ApplyImpulse(mult * Stats.PlayerStats.DamageRecoil.GetDynamicVal() * (GlobalPosition - recoilFrom).Normalized() * (recoilRange - distance));
            if (Stats.PlayerStats.RevengeDamage.GetDynamicVal() > 0)
            {
                TakeDamage(Stats.PlayerStats.RevengeDamage.GetDynamicVal() * Stats.PlayerStats.Damage.GetDynamicVal());
            }
        }
    }

    protected float GetPoisonInterval()
    {
        return 1 / Mathf.Pow(venomFrequency.GetDynamicVal(), 0.5f);
    }

    public void SetExplodeOnDeath(bool b)
    {
        explodeOnDeath = b;
    }

    public void TakeDamage(float dmg)
    {
        if (Venom.unlocked && !poisoned)
        {
            Poison();
        }
        ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(State.MobDamage, Colors.Red, 1f, 100, 0.1f));

        Hud hud = GetParent().GetNode<Hud>("HUD");
        hud.CreateDamageNumber(Position, dmg);
        damageSound.Play();
        hp -= dmg;
        if (hp <= 0)
        {

            Die();
        }

    }

    public float GetHP()
    {
        return hp;
    }


    protected virtual void Die()
    {
        
        // Don't queuefree yet, that happens once the damage sound is complete
        dead = true;
        Hide();
        CollisionLayer = 0;
        CollisionMask = 0;


    }

    protected void Explode()
    {
        if (DeathExplosion.unlocked)
        {
            Explosion explosion = State.sceneHolder.explosion.Instantiate<Explosion>();
            explosion.GlobalPosition = GlobalPosition;
            explosion.Appear();
            GetTree().Root.CallDeferred(Node2D.MethodName.AddChild, explosion);
        }
        if (PlagueExplosion.unlocked)
        {
            PlagueCloud cloud = State.sceneHolder.plagueCloud.Instantiate<PlagueCloud>();
            cloud.GlobalPosition = GlobalPosition;
            cloud.Appear();
            GetTree().Root.CallDeferred(Node2D.MethodName.AddChild, cloud);
        }
    }

    protected void SpawnCoin()
    {
        Coin newCoin = State.sceneHolder.coin.Instantiate<Coin>();
        newCoin.Position = Position + new Vector2(GD.Randf() * 2 - 1, GD.Randf() * 2 - 1) * 20;
        GetParent().CallDeferred("add_child", newCoin);

    }
    

    public void Poison()
    {
        if (poisoned)
        {
            return;
        }
        else
        {
            poisoned = true;
            poisonTick = GetPoisonInterval() * (GD.Randf() + 0.5f); // Adding a little delay makes disease less grating when it infects a bunch at once
            if (PlagueExplosion.unlocked && GD.Randf() < plagueExplosionChance.GetDynamicVal())
            {
                explodeOnDeath = true;
            }
            if (Plague.unlocked)
            {
                ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(Plague, Colors.Purple, 1, 5));

            }
            else
            {
                ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(Venom, Colors.Green, 0.8f, 4));

            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Pause logic. NOTHING GOES BEFORE THIS UNLESS IT SHOULD BE HAPPENING EVEN WHILE PAUSED
        if (State.currentState == State.paused)
        {

            Pause();
            return;
        }
        if (poisoned)
        {
            poisonTick -= (float)delta;
            ProcessPoison();
        }
        if (beforePauseVelocity != Vector2.Zero)
        {
            UnPause();
            beforePauseVelocity = Vector2.Zero;

        }

        ProcessMovement(delta);



        ((IAffectedByVisualEffects)this).ProcessVisualEffects((float)delta);

    }

    protected abstract void ProcessMovement(double delta);

    void ProcessPoison()
    {
        if (poisonTick < 0)
        {
            poisonTick = GetPoisonInterval();
            TakeDamage(venomDamage.GetDynamicVal());
            if (Plague.unlocked)
            {
                // Check for any mobs I'm touching and poison them
                foreach (Node2D touching in GetCollidingBodies())
                {
                    if (touching is Mob)
                    {
                        ((Mob)touching).Poison();
                    }
                }
                if (LightningPlague.unlocked)
                {
                    SpawnLightning(venomDamage.GetDynamicVal(), this, 0, State.sceneHolder.lightningArc);
                }
            }

        }
    }





    private void UnPause()
    {
        Sleeping = false;
        LinearVelocity = beforePauseVelocity;
        AngularVelocity = beforePauseAngularVelocity;
    }


    private void Pause()
    {
        if (beforePauseVelocity == Vector2.Zero)
        {
            beforePausePosition = Position;
            beforePauseVelocity = LinearVelocity;
            beforePauseAngularVelocity = AngularVelocity;
        }
        Sleeping = true;
        LinearVelocity = Vector2.Zero;

        AngularVelocity = 0;
    }


    private void CreateIndicator()
    {
        pairedIndicator = offscreenIndicator.Instantiate<OffscreenIndicator>();
        pairedIndicator.SetMobParent(this);
        GetTree().Root.AddChild(pairedIndicator);
        pairedIndicator.Position = Position;
        pairedIndicator.GetNode<Sprite2D>("Sprite2D").Scale = GetIndicatorSize();
    }

    protected virtual Vector2 GetIndicatorSize()
    {
        return Vector2.One * size;
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
