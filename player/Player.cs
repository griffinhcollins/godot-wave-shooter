using Godot;
using static Stats.PlayerStats;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Player : Node2D, IAffectedByVisualEffects
{

    [Signal]
    public delegate void KilledEventHandler();

    [Export]
    public PackedScene bounceBullet;
    [Export]
    public PackedScene pierceBullet;
    [Export]
    public PackedScene laserBeam;
    [Export]
    public PackedScene fireTrail;


    CpuParticles2D bubbleEmitter;
    float emitterX;

    float damage; // Default damage per shot
    float firingspeed; // Shots/second
    int currentHP;


    public Vector2 screenSize; // pixel screen size

    AnimatedSprite2D sprite;

    Node2D reticule;

    Timer bulletTimer;
    Timer trailTimer;

    Hud hud;

    Vector2 prevVelocity;

    AudioStreamPlayer2D damageSound;

    Node2D shield;
    bool shieldActive;

    RayCast2D[] raycasters;
    float[] laserLastDamageTime;

    bool canFire = true;

    // For Charged shots
    int shotsCharged = 0;
    float timeCharging = 0;


    public List<VisualEffect> visualEffects { get; set; }
    public Dictionary<StaticColourChange, float> staticColours { get; set; }
    public Dictionary<ParticleEffect, GpuParticles2D> instantiatedParticles { get; set; } // Particle effects should only instantiate once
    public HashSet<Improvement> overwrittenSources { get; set; }

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {

        State.player = this;
        Hide();
        screenSize = GetViewportRect().Size;
        sprite = GetNode<AnimatedSprite2D>("MainSprite");
        bubbleEmitter = GetNode<CpuParticles2D>("BubbleEmitter");
        emitterX = bubbleEmitter.Position.X;
        reticule = GetNode<Node2D>("ReticuleHolder");
        bulletTimer = GetNode<Timer>("BulletTimer");
        trailTimer = GetNode<Timer>("TrailTimer");

        damageSound = GetNode<AudioStreamPlayer2D>("DamageSound");
        hud = GetParent().GetNode<Hud>("HUD");
        shield = GetNode<Node2D>("Shield");



        // Hide();
    }


    public void UpdateStats()
    {
        timeCharging = 0;
        shotsCharged = 0;

        firingspeed = FireRate.GetDynamicVal();
        if (Unlocks.Laser.unlocked)
        {
            int beamnum = Mathf.FloorToInt(Multishot.GetDynamicVal());
            bulletTimer.WaitTime = 1 / (firingspeed * 5);
            raycasters = new RayCast2D[beamnum];
            laserLastDamageTime = new float[beamnum];

            foreach (Node2D child in reticule.GetChildren())
            {
                if (child is RayCast2D)
                {
                    child.QueueFree();
                }
            }
            for (int i = 0; i < beamnum; i++)
            {
                // Add a raycasting laser
                RayCast2D raycaster = new RayCast2D();
                CircleShape2D castCircle = new CircleShape2D();
                castCircle.Radius = BulletSize.GetDynamicVal() * 10;
                // raycaster.Shape = castCircle;
                raycaster.SetCollisionMaskValue(1, false);
                raycaster.SetCollisionMaskValue(2, true);
                // raycaster.SetCollisionMaskValue(5, true);
                raycaster.Enabled = true; // we don't want it to cast every frame, just when we ask it to
                raycaster.CollideWithAreas = true;
                raycaster.TargetPosition = Vector2.Up * 1000;
                reticule.AddChild(raycaster);
                raycasters[i] = raycaster;
                laserLastDamageTime[i] = 0;
            }
        }
        else
        {

            bulletTimer.WaitTime = 1 / firingspeed;
        }
        currentHP = (int)MaxHP.GetDynamicVal();
        hud.UpdateHealth(currentHP);

    }


    public void AddMoney(int amount)
    {
        Money += amount;
        Money = Mathf.Min(Money, (int)MoneyCap.GetDynamicVal());
        hud.UpdateMoneyCounter(Money);
    }

    public bool ChargeMoney(int amount)
    {
        if (Money < amount)
        {
            return false;
        }
        else
        {
            Money -= amount;
            hud.UpdateMoneyCounter(Money);
            return true;
        }
    }



    private void OnBulletTimerFinished()
    {
        canFire = true;
    }

    private void ShootLaser(Vector2 offset)
    {
        if (State.currentState != State.alive)
        {
            return;
        }
        int beamCount = raycasters.Length;
        for (int i = 0; i < beamCount; i++)
        {
            float rotation = 0;
            if (raycasters.Length > 1)
            {
                rotation = (i / (beamCount - 1f) - 0.5f) * Spread.GetDynamicVal() * beamCount * 0.0174533f;
                // GD.Print(i);
                // GD.Print(rotation);
            }
            RayCast2D raycaster = raycasters[i];
            Vector2 pointVec = GetGlobalMousePosition() - Position;
            raycaster.GlobalRotation = -1 * pointVec.AngleTo(Vector2.Up) + rotation;
            raycaster.GlobalPosition = GlobalPosition;
            // raycaster.ForceRaycastUpdate();
            float distanceToHit = 1000;
            if (raycaster.IsColliding())
            {
                Node2D hit = (Node2D)raycaster.GetCollider();
                distanceToHit = (hit.GlobalPosition - raycaster.GlobalPosition).Length();
                if (hit is Mob && laserLastDamageTime[i] > 1 / (FireRate.GetDynamicVal() * 5f))
                {
                    GD.Print(i);
                    laserLastDamageTime[i] = 0f;

                    ((Mob)hit).TakeDamage(Damage.GetDynamicVal() / 5f, DamageTypes.Laser);
                }
            }
            State.bulletManager.FireLaser(new Vector2(0, -1).Rotated(raycaster.GlobalRotation), offset + GlobalPosition, distanceToHit, i);
        }

        // Area2D newBeam = laserBeam.Instantiate<Area2D>();
        // newBeam.Position = fireFromPos;
        // newBeam.Rotate(reticule.Rotation);
        // // projectile = newBeam.GetNode<LaserBeam>("ScriptHolder");
        // AddChild(newBeam);
    }

    private void ShootBullet(Vector2 fireFromPos)
    {


        float spreadRotate = reticule.Rotation + Mathf.DegToRad(Spread.GetDynamicVal()) * (float)GD.RandRange(-1, 1f);

        Bullet projectile;


        // PhysicsServer implementation
        projectile = State.bulletManager.SpawnBullet(new Vector2(0, -1).Rotated(spreadRotate), fireFromPos + Position);
        State.audioManager.PlaySound("FireSound");
        // State.bulletManager.NewSpawn(new Vector2(0, -1).Rotated(spreadRotate), 1000 * ShotSpeed.GetDynamicVal(), fireFromPos + Position);
        return;





    }

    public void EndWave()
    {
        trailTimer.Stop();
        GetNode<CpuParticles2D>("DamageEffect").Hide();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        bool isPaused = State.currentState == State.paused;
        bulletTimer.Paused = isPaused;
        trailTimer.Paused = isPaused;
        if (State.currentState != State.alive)
        {

            return;
        }

        ProcessMovement((float)delta);
        // Point reticule at mouse
        Vector2 pointVec = GetGlobalMousePosition() - Position;
        reticule.GlobalRotation = -1 * pointVec.AngleTo(Vector2.Up);



        // Regular Fire if clicking 
        if (Unlocks.Laser.unlocked)
        {
            for (int i = 0; i < laserLastDamageTime.Length; i++)
            {
                laserLastDamageTime[i] += (float)delta;
            }
            if (Input.IsActionPressed("fire"))
            {

                ShootLaser(new Vector2(0, 0));
            }
            else
            {
                State.bulletManager.StopLaser();
            }
        }
        else
        {

            if (Input.IsActionPressed("fire") && canFire)
            {
                canFire = false;
                bulletTimer.Start();
                Vector2 fireFromPos = new Vector2(0, 0);
                // Multishot
                float shots = Multishot.GetDynamicVal();
                while (shots > 0)
                {
                    if (GD.RandRange(0f, 1) <= shots)
                    {
                        if (ChargeTime.GetDynamicVal() <= 0)
                        {
                            ShootBullet(fireFromPos);

                        }
                        else if (timeCharging <= ChargeTime.GetDynamicVal())
                        {
                            shotsCharged++;
                            ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(ChargeTime, Colors.Red, 0.5f, 1, 0.1f));
                        }
                    }
                    fireFromPos += new Vector2(10 + (GD.Randi() % 5), 0).Rotated(reticule.Rotation) * (GD.Randi() % 2 == 0 ? 1 : -1);
                    shots--;
                }

            }

            if (ChargeTime.GetDynamicVal() > 0)
            {
                if (Input.IsActionPressed("fire"))
                {

                    timeCharging += (float)delta;
                    if (timeCharging > ChargeTime.GetDynamicVal() && timeCharging - (float)delta <= ChargeTime.GetDynamicVal())
                    {
                        // flash to indicate the bullets have finished charging

                        ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(ChargeTime, Colors.Green, 1f, 100, 0.1f));
                    }
                }
                else if (timeCharging > 0)
                {
                    FireChargedShots();
                }

            }
        }

        ((IAffectedByVisualEffects)this).ProcessVisualEffects((float)delta);



    }

    async Task FireChargedShots()
    {
        timeCharging = 0;
        Vector2 fireFromPos = Vector2.Zero;
        for (int i = 0; i < shotsCharged; i++)
        {

            ShootBullet(fireFromPos);

            fireFromPos += new Vector2(10 + (GD.Randi() % 5), 0).Rotated(reticule.Rotation) * (GD.Randi() % 2 == 0 ? 1 : -1);
            await ToSignal(GetTree().CreateTimer(GD.Randf() * 0.1f / shotsCharged), SceneTreeTimer.SignalName.Timeout);
        }
        shotsCharged = 0;
    }



    void ProcessMovement(float delta)
    {
        // Be still by default
        Vector2 velocity = Vector2.Zero;

        // Check inputs
        if (Input.IsActionPressed("move_right"))
        {
            velocity.X += 1;
        }

        if (Input.IsActionPressed("move_left"))
        {
            velocity.X -= 1;
        }

        if (Input.IsActionPressed("move_down"))
        {
            velocity.Y += 1;
        }

        if (Input.IsActionPressed("move_up"))
        {
            velocity.Y -= 1;
        }

        if (velocity.LengthSquared() > 0)
        {
            velocity = velocity.Normalized() * Speed.GetDynamicVal();
            sprite.Play();
        }
        else
        {
            sprite.Stop();
        }

        // Move
        Position += velocity * delta;
        Position = new Vector2(Mathf.Clamp(Position.X, 0, screenSize.X), Mathf.Clamp(Position.Y, 0, screenSize.Y));



        // Update animations
        bubbleEmitter.Emitting = velocity.LengthSquared() > 0;
        if (velocity.X != 0)
        {
            sprite.Animation = "jupes_sub";
            sprite.FlipV = false;
            sprite.FlipH = velocity.X > 0;
            bubbleEmitter.Position = new Vector2(emitterX * (velocity.X > 0 ? -1 : 1), bubbleEmitter.Position.Y);
            bubbleEmitter.InitialVelocityMin = 100 * (velocity.X > 0 ? -1 : 1);
            bubbleEmitter.InitialVelocityMax = 100 * (velocity.X > 0 ? -1 : 1);

        }
        else if (velocity.Y != 0)
        {
            sprite.Animation = "jupes_sub";
            // sprite.FlipV = velocity.Y > 0;
        }

        // Point where we're going
        float targetRotation;
        if (velocity.Y == 0 || velocity.X == 0)
        {
            targetRotation = 0;
        }
        else
        {
            if (velocity.X > 0)
            {
                if (velocity.Y > 0)
                {
                    targetRotation = 1;
                }
                else
                {
                    targetRotation = -1;
                }
            }
            else
            {
                if (velocity.Y > 0)
                {
                    targetRotation = -1;
                }
                else
                {
                    targetRotation = 1;
                }
            }
        }
        Rotation = Mathf.Lerp(Rotation, targetRotation / 3f, delta * 5);

        // If we were facing left and are now facing right, or vice versa, flip rotation immediately
        if (prevVelocity.X >= 0 != velocity.X >= 0 && velocity.X != 0)
        {
            Rotation = -Rotation;
        }

        if (velocity.X != 0)
        {
            prevVelocity = velocity;

        }
    }




    private void Die()
    {
        Money = 0;
        EmitSignal(SignalName.Killed);
        ToggleCollision(false);
        trailTimer.Stop();
        Hide();
        GetNode<CpuParticles2D>("DamageEffect").Hide();
    }

    public int CurrentHP()
    {
        return currentHP;
    }

    private async void OnBodyEntered(Node2D body)
    {
        // GD.Print("ouch!");
        // freeze the mob that hit me unless it's a biter
        if (body is Mob && body is not Biter)
        {
            ((Mob)body).LinearVelocity = Vector2.Zero;
        }
        // tell mobs to back off
        foreach (Mob mob in GetTree().GetNodesInGroup("mobs"))
        {
            mob.Recoil(GlobalPosition);
        }
        // push away bombs
        foreach (Bomb b in GetTree().GetNodesInGroup("bombs"))
        {
            float dist = (b.GlobalPosition - GlobalPosition).Length();
            if (dist < 400)
            {
                b.ApplyImpulse((b.GlobalPosition - GlobalPosition) / dist * (400 - dist));

            }
        }

        bool shielded = false;
        float iframes = 0.5f;

        if (!shieldActive)
        {
            currentHP--;
            damageSound.Play();
            hud.ShowDamageEffect();
            GetNode<CpuParticles2D>("DamageEffect").Show();
            GetNode<CpuParticles2D>("DamageEffect").Emitting = true;
            ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(State.MobDamage, Colors.Red, 0.8f, 100, iframes));
            hud.UpdateHealth(currentHP);
            if (currentHP <= 0)
            {

                Die();
                return;
            }

        }
        else
        {
            // Shield took the hit
            shieldActive = false;
            shield.Hide();
            shielded = true;

        }

        // Activate I-frames
        // Need to use deferred because this is called on a physics callback, and can't edit physics properties in a physics callback
        ToggleCollision(false);
        await ToSignal(GetTree().CreateTimer(iframes), SceneTreeTimer.SignalName.Timeout);
        ToggleCollision(true);

        // Wait for iframes to be over before beginning countdown for shield to prevent shenanigans
        if (shielded)
        {
            int currentWave = Stats.Counters.WaveCounter.Value;
            await ToSignal(GetTree().CreateTimer(Unlocks.shieldRecharge.GetDynamicVal()), SceneTreeTimer.SignalName.Timeout);
            if (currentWave != Stats.Counters.WaveCounter.Value)
            {
                // The shield started recharging last wave or we died since then, either way don't raise the shield
                return;
            }
            RaiseShieldIfUnlocked();
        }



    }

    void RaiseShieldIfUnlocked()
    {
        // GD.Print("raising shield");
        if (!Unlocks.Shield.unlocked)
        {
            shield.Hide();
            shieldActive = false;
            return;
        }

        shieldActive = true;

        shield.Show();

    }



    public void Start(Vector2 position)
    {
        UpdateStats();
        Position = position;
        Show();
        ToggleCollision(true);
        bubbleEmitter.Amount = bubbleEmitter.Amount; // Doing this clears existing particles

        if (Unlocks.FireTrail.unlocked)
        {
            trailTimer.WaitTime = 0.25f / Unlocks.trailDensity.GetDynamicVal();
            trailTimer.Start();
        }
        else
        {
            trailTimer.Stop();
        }
        if (Unlocks.Flamethrower.unlocked)
        {
            Mutations.SetMutation(Mutations.GrowingBullet);
        }

        RaiseShieldIfUnlocked();

        ((IAffectedByVisualEffects)this).ImmediateVisualEffects();
    }

    void ToggleCollision(bool toggle)
    {
        GetNode<CollisionShape2D>("BodyCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, !toggle);
        GetNode<CollisionShape2D>("PropellerCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, !toggle);
        GetNode<CollisionPolygon2D>("FinCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, !toggle);
    }


    private void OnTrailTimeout()
    {
        FireTrail t = fireTrail.Instantiate<FireTrail>();
        GetTree().Root.AddChild(t);
        t.GlobalPosition = GlobalPosition;
    }


}
