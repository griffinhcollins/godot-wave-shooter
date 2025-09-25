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

    // Ignition
    bool ignited = false;
    float igniteTick;

    protected int mobNum;


    // Explosions!
    protected bool explodeOnDeath = false;

    protected float hp;

    public bool staticApplied = false;
    public bool stunned = false;


    protected Vector2 beforePauseVelocity;
    protected Vector2 beforePausePosition;
    protected float beforePauseAngularVelocity;

    public bool dead = false;

    GpuParticles2D staticParticles;


    [Export]
    public int firstAppearsAtWave;


    protected float size;

    protected OffscreenIndicator pairedIndicator;

    AudioStreamPlayer2D damageSound;

    protected VisibleOnScreenNotifier2D onScreenNotifier2D;


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
        string[] colour = animSprite.SpriteFrames.GetAnimationNames();
        animSprite.Play(colour[GD.Randi() % colour.Length]);
        damageSound = GetNode<AudioStreamPlayer2D>("DamageSound");
        onScreenNotifier2D = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        // Set Size
        SetSize();
        SetScale();
        InitialMovement();
        mobNum = Stats.Counters.EnemyCounter.Value++;
        if (DeathExplosion.unlocked)
        {
            if (GD.Randf() < explosionChance.GetDynamicVal())
            {
                explodeOnDeath = true;
            }
        }




        ((IAffectedByVisualEffects)this).ImmediateVisualEffects();

    }


    protected abstract void InitialMovement();

    public virtual Vector2 GetIndicatorSize()
    {
        return Vector2.One * size;
    }

    abstract public string GetMobName();

    public string GetID()
    {
        return GetMobName() + mobNum;
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
        Node2D collider = GetNode<Node2D>("Collider");
        collider.Scale *= size;
    }

    protected virtual float GetBaseHealth()
    {
        return 20;
    }


    protected void OnCollision(Node2D node)
    {
        // GD.Print(node.Name);
        // GD.Print("body");
    }

    protected void OnCollision(Rid rid, Node body, int i1, int i2)
    {
        // GD.Print(body.Name);
        // GD.Print("bunch");
    }

    // Used when the player is hit to give them some breathing room
    public virtual void Recoil(Vector2 recoilFrom, float mult = 1)
    {
        float recoilRange = 800;
        if ((GlobalPosition - recoilFrom).LengthSquared() < recoilRange * recoilRange)
        {
            float distance = (GlobalPosition - recoilFrom).Length();
            if (this is not Bomber)
            {
                LinearVelocity = Vector2.Zero;

            }
            ApplyImpulse(mult * Stats.PlayerStats.DamageRecoil.GetDynamicVal() * (GlobalPosition - recoilFrom).Normalized() * (recoilRange - distance));
            if (Stats.PlayerStats.RevengeDamage.GetDynamicVal() > 0)
            {
                TakeDamage(Stats.PlayerStats.RevengeDamage.GetDynamicVal() * Stats.PlayerStats.Damage.GetDynamicVal(), DamageTypes.Blunt);
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

    public void TakeDamage(float dmg, DamageType type, bool playSound = true)
    {
        if (Venom.unlocked && !poisoned)
        {
            Poison();
        }

        if (!ignited && type == DamageTypes.Fire && GD.Randf() <= Stats.PlayerStats.IgnitionChance.GetDynamicVal())
        {
            Ignite();
        }

        Hud hud = GetParent().GetNode<Hud>("HUD");
        dmg = dmg * DamageResistanceMult(type);
        hud.CreateDamageNumber(Position, dmg);
        if (playSound)
        {
            damageSound.Play();

        }
        if (dmg > 0)
        {
            ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(State.MobDamage, Colors.Red, 1f, 100, 0.1f));
            hp -= dmg;
            if (hp <= 0)
            {

                Die();
            }
        }


    }

    protected void Ignite()
    {
        if (ignited)
        {
            return;
        }

        Node2D fireParticles = State.sceneHolder.ignitedParticles.Instantiate<Node2D>();
        // Take damage very rapidly
        AddChild(fireParticles);
        ignited = true;
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
        this.RemoveFromGroup("mobs");


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


    public virtual void Poison()
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
        if (dead)
        {
            return;
        }
        if (poisoned)
        {
            poisonTick -= (float)delta;
            ProcessPoison();
        }
        if (ignited)
        {
            igniteTick += (float)delta;
            ProcessIgnition();
        }
        if (beforePauseVelocity != Vector2.Zero)
        {
            UnPause();
            beforePauseVelocity = Vector2.Zero;

        }





        ((IAffectedByVisualEffects)this).ProcessVisualEffects((float)delta);

    }

    public override void _PhysicsProcess(double delta)
    {
        // Pause logic. NOTHING GOES BEFORE THIS UNLESS IT SHOULD BE HAPPENING EVEN WHILE PAUSED
        if (State.currentState == State.paused)
        {

            Pause();
            return;
        }
        if (dead)
        {
            return;
        }
        double time = Time.GetUnixTimeFromSystem();
        ProcessMovement(delta);
        time -= Time.GetUnixTimeFromSystem();
        time *= -1;
        if (time > 0.005)
        {
            GD.Print(string.Format("{0} Movement time: {1}ms", GetID(), time * 1000));
        }
    }


    protected abstract void ProcessMovement(double delta);
    protected void ProcessIgnition()
    {
        if (igniteTick > GD.Randf())
        {
            TakeDamage(1, DamageTypes.Fire, false);
            igniteTick = 0;
        }
    }

    protected virtual float DamageResistanceMult(DamageType t)
    {
        return 1;
    }

    void ProcessPoison()
    {
        if (poisonTick < 0)
        {
            poisonTick = GetPoisonInterval();
            TakeDamage(venomDamage.GetDynamicVal(), DamageTypes.Poison);
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
                if (LightningPlague.unlocked && GD.Randf() < lightningPlagueChance.GetDynamicVal())
                {
                    SpawnLightning(venomDamage.GetDynamicVal(), this, 1);
                }
            }

        }
    }

    async public virtual void ApplyStatic() // While under the effects of static, mobs can't be hit by lightning arcs but (if upgraded) can be stunned
    {
        if (staticApplied)
        {
            return;
        }
        staticApplied = true;
        if (staticParticles is null)
        {
            staticParticles = State.sceneHolder.lightningStaticParticles.Instantiate<GpuParticles2D>();
            AddChild(staticParticles);

        }
        staticParticles.Show();

        if (GD.Randf() <= lightningStunChance.GetDynamicVal())
        {
            stunned = true;
            ((IAffectedByVisualEffects)this).AddStaticColour(this, new StaticColourChange(lightningStunChance, Colors.DeepSkyBlue, 1, 10, lightningStaticTimeDown.GetDynamicVal()));
            animSprite.SpeedScale = 0.1f;
            GD.Print("stunned!");
        }

        await ToSignal(GetTree().CreateTimer(lightningStaticTimeDown.GetDynamicVal()), SceneTreeTimer.SignalName.Timeout); // countdown before coming off lightning cooldown

        animSprite.SpeedScale = 1;
        staticApplied = false;
        stunned = false;
        staticParticles.Hide();
    }





    protected virtual void UnPause()
    {
        Sleeping = false;
        LinearVelocity = beforePauseVelocity;
        AngularVelocity = beforePauseAngularVelocity;
    }


    protected virtual void Pause()
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











    private void OnDamageSoundFinished()
    {
        if (dead)
        {
            QueueFree();
        }
    }


}
