using System;
using static Stats.EnemyStats;
using Godot;
using System.Threading.Tasks;

public abstract partial class Mob : RigidBody2D
{

    Player player;

    // Venom
    bool poisoned = false;
    float poisonTick;

    // Plague Explosion
    bool explodeOnDeath = false;

    float baseHealth = 20;
    float baseSpeedLimit = 400;
    float baseAcceleration = 1;
    float hp;
    float speedLimit;
    float acceleration;


    protected Vector2 beforePauseVelocity;
    protected Vector2 beforePausePosition;
    protected float beforePauseAngularVelocity;

    bool dead = false;

    Node2D eye;

    [Export]
    PackedScene offscreenIndicator;




    OffscreenIndicator pairedIndicator;

    AudioStreamPlayer2D damageSound;



    float size;

    AnimatedSprite2D animSprite;

    CollisionShape2D collider;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {





        acceleration = baseAcceleration * DynamicStats[ID.AccelerationMult];
        player = (Player)GetTree().GetNodesInGroup("player")[0];
        speedLimit = baseSpeedLimit;
        animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        string[] mobTypes = animSprite.SpriteFrames.GetAnimationNames();
        animSprite.Play(mobTypes[GD.Randi() % mobTypes.Length]);
        damageSound = GetNode<AudioStreamPlayer2D>("DamageSound");

        // Set Size
        size = GD.Randf() * 0.5f * DynamicStats[ID.SizeMult] + 1;
        hp = DynamicStats[ID.HPMult] * baseHealth * size * 0.75f;
        animSprite.Scale *= size;
        collider = GetNode<CollisionShape2D>("CollisionShape2D");
        collider.Scale *= size;
        eye = GetNode<Node2D>("Eye");
        eye.Scale *= size;


        animSprite.SpeedScale = 1 / (size);
        CreateIndicator();
    }


    // Used when the player is hit to give them some breathing room
    public void Recoil(Vector2 playerPosition)
    {
        float recoilRange = 800;
        if ((GlobalPosition - playerPosition).LengthSquared() < recoilRange * recoilRange)
        {
            float distance = (GlobalPosition - playerPosition).Length();
            LinearVelocity = Vector2.Zero;
            ApplyImpulse((GlobalPosition - playerPosition).Normalized() * (recoilRange - distance));

        }
    }

    protected float GetPoisonInterval()
    {
        return 1 / Mathf.Pow(Stats.PlayerStats.Unlocks.venomFrequency.GetDynamicVal(), 0.5f);
    }

    public void SetExplodeOnDeath(bool b)
    {
        explodeOnDeath = b;
    }

    public async Task TakeDamage(float dmg)
    {
        if (Stats.PlayerStats.Unlocks.Venom.unlocked && !poisoned)
        {
            Poison();
        }
        animSprite.Modulate = Color.Color8(255, 0, 0);

        Hud hud = GetParent().GetNode<Hud>("HUD");
        hud.CreateDamageNumber(Position, dmg);
        damageSound.Play();
        hp -= dmg;
        if (hp <= 0)
        {
            Die();
        }
        else
        {
            await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
            animSprite.Modulate = Color.Color8(255, 255, 255);
        }

    }

    public float GetHP()
    {
        return hp;
    }


    private void Die()
    {
        Stats.Counters.KillCounter.Value++;
        dead = true;
        float tempDropRate = Stats.PlayerStats.DropRate.GetDynamicVal();
        // If drop rate is above 1, get 1 guaranteed coin plus a chance at another
        while (tempDropRate > 0)
        {
            if (GD.RandRange(0f, 1) <= tempDropRate)
            {
                SpawnCoin();
            }
            tempDropRate--;
        }
        if (explodeOnDeath)
        {
            PlagueCloud cloud = State.sceneHolder.plagueCloud.Instantiate<PlagueCloud>();
            cloud.GlobalPosition = GlobalPosition;
            cloud.Appear();
            GetTree().Root.CallDeferred(Node2D.MethodName.AddChild, cloud);
        }
        // Don't queuefree yet, that happens once the damage sound is complete
        Hide();
        CollisionLayer = 0;
        CollisionMask = 0;


    }

    private void SpawnCoin()
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
            if (Stats.PlayerStats.Unlocks.PlagueExplosion.unlocked && GD.Randf() < Stats.PlayerStats.Unlocks.plagueExplosionChance.GetDynamicVal())
            {
                explodeOnDeath = true;
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Pause logic. NOTHING GOES BEFORE THIS UNLESS IT SHOULD BE HAPPENING EVEN WHILE PAUSED
        if (State.currentState == State.paused)
        {
            if (beforePauseVelocity == Vector2.Zero)
            {
                beforePausePosition = Position;
                beforePauseVelocity = LinearVelocity;
                beforePauseAngularVelocity = AngularVelocity;
            }
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
        // Point towards the player
        ApplyForce((player.Position - Position) * ((player.Position - Position).Length() * 1 / 1000 + speedLimit / 500) * acceleration);
        ApplyTorque(LinearVelocity.AngleTo(ToGlobal(Vector2.Up)) * 1000);
        float currentSpeedSq = LinearVelocity.LengthSquared();
        if (currentSpeedSq > speedLimit * speedLimit)
        {
            LinearVelocity = LinearVelocity.Normalized() * speedLimit;
            // ApplyForce(LinearVelocity * (speedLimit - Mathf.Pow(currentSpeedSq, 0.5f)));
        }

        GazeAt(player.GlobalPosition, (float)delta);

    }

    void ProcessPoison()
    {
        if (poisonTick < 0)
        {
            poisonTick = GetPoisonInterval();
            TakeDamage(Stats.PlayerStats.Unlocks.venomDamage.GetDynamicVal());
            if (Stats.PlayerStats.Unlocks.Plague.unlocked)
            {
                // Check for any mobs I'm touching and poison them
                foreach (Node2D touching in GetCollidingBodies())
                {
                    if (touching is Mob)
                    {
                        ((Mob)touching).Poison();
                    }
                }
                if (Stats.PlayerStats.Unlocks.LightningPlague.unlocked)
                {
                    Stats.PlayerStats.Unlocks.SpawnLightning(Stats.PlayerStats.Unlocks.venomDamage.GetDynamicVal(), this, 0, State.sceneHolder.lightningArc);
                }
            }

        }
    }

    protected abstract float GetIrisMoveRadius();
    protected abstract float GetPupilMoveRadius();

    void GazeAt(Vector2 targetPos, float delta)
    {

        float irisMoveRadius = GetIrisMoveRadius();
        float pupilMoveRadius = GetPupilMoveRadius();

        eye.GlobalPosition = eye.GlobalPosition.Lerp((targetPos - GlobalPosition).Normalized() * irisMoveRadius + GlobalPosition, delta * 3);


        Node2D pupil = eye.GetNode<Node2D>("Pupil");


        pupil.GlobalPosition = pupil.GlobalPosition.Lerp((targetPos - eye.GlobalPosition).Normalized() * pupilMoveRadius + eye.GlobalPosition, delta * 5);

        // float oldRot = eye.Rotation;
        // eye.LookAt(targetPos);
        // float targetRot = eye.Rotation;
        // eye.Rotation = Mathf.Lerp(oldRot, targetRot, delta * 10);
    }


    private void UnPause()
    {
        Sleeping = false;
        LinearVelocity = beforePauseVelocity;
        AngularVelocity = beforePauseAngularVelocity;
    }


    private void Pause()
    {
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
        pairedIndicator.GetNode<Sprite2D>("Sprite2D").Scale *= size;
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
