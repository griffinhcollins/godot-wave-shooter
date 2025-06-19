using Godot;
using static Stats.PlayerStats;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Collections.Generic;
using System.Linq;

public partial class Player : Area2D, IAffectedByVisualEffects
{

    [Signal]
    public delegate void KilledEventHandler();

    [Export]
    public PackedScene bounceBullet;
    [Export]
    public PackedScene pierceBullet;
    [Export]
    public PackedScene laserBeam;


    CpuParticles2D bubbleEmitter;
    float emitterX;

    float damage; // Default damage per shot
    float firingspeed; // Shots/second
    int currentHP;


    public Vector2 screenSize; // pixel screen size

    AnimatedSprite2D sprite;

    Node2D reticule;

    Timer bulletTimer;

    Hud hud;

    Vector2 prevVelocity;

    List<Mutation> activeMutations;





    bool canFire = true;

    public List<VisualEffect> visualEffects { get; set; }
    public Dictionary<StaticColourChange, float> staticColours { get; set; }
    public HashSet<Improvement> overwrittenSources { get; set; }

    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {

        Hide();
        screenSize = GetViewportRect().Size;
        sprite = GetNode<AnimatedSprite2D>("MainSprite");
        bubbleEmitter = GetNode<CpuParticles2D>("BubbleEmitter");
        emitterX = bubbleEmitter.Position.X;
        reticule = GetNode<Node2D>("ReticuleHolder");
        bulletTimer = GetNode<Timer>("BulletTimer");

        hud = GetParent().GetNode<Hud>("HUD");


        // Hide();
    }


    public void UpdateStats()
    {


        firingspeed = FireRate.GetDynamicVal();
        bulletTimer.WaitTime = 1 / firingspeed;
        currentHP = (int)MaxHP.GetDynamicVal();
        hud.UpdateHealth(currentHP);

        activeMutations = Mutations.allMutations.Where(m => m.applied).ToList();
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

    private void ShootBullet(Vector2 fireFromPos)
    {
        if (State.currentState != State.alive)
        {
            return;
        }
        canFire = false;
        bulletTimer.Start();

        float spreadRotate = reticule.Rotation + Mathf.DegToRad(Spread.GetDynamicVal()) * (float)GD.RandRange(-1, 1f);

        Bullet projectile;
        Node2D projectileHolder;

        if (Unlocks.Laser.unlocked)
        {
            Area2D newBeam = laserBeam.Instantiate<Area2D>();
            newBeam.Position = fireFromPos;
            newBeam.Rotate(spreadRotate);
            projectile = newBeam.GetNode<LaserBeam>("ScriptHolder");
            AddChild(newBeam);
        }
        else
        {
            fireFromPos += Position; // Add the player's position because this projectile isn't a child of this node
            // A bullet is piercing first, then when piercing runs out becomes bouncy. If there is no piercing, it starts bouncy.
            Vector2 velocity = new Vector2(0, -1000 * ShotSpeed.GetDynamicVal()).Rotated(spreadRotate);

            if (!Unlocks.PiercingBullets.unlocked)
            {
                // Bouncing, it's a rigidbody
                projectileHolder = bounceBullet.Instantiate<RigidBody2D>();
            }
            else
            {
                // Piercing, it's an Area2D
                projectileHolder = pierceBullet.Instantiate<Area2D>();
            }

            projectileHolder.Rotate(spreadRotate);
            projectile = projectileHolder.GetNode<Bullet>("ScriptHolder");
            projectile.originalBullet = true;
            projectile.SetVelocity(velocity);
            projectileHolder.Position = fireFromPos;
            GetParent().AddChild(projectileHolder);


        }

        if (activeMutations is not null)
        {
            foreach (Mutation m in activeMutations)
            {
                projectile.AddMutation(m);
            }

        }

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        if (State.currentState == State.paused)
        {
            return;
        }

        // Point reticule at mouse
        Vector2 pointVec = GetGlobalMousePosition() - Position;
        reticule.Rotation = -1 * pointVec.AngleTo(Vector2.Up);

        ProcessMovement((float)delta);


        // Regular Fire if clicking 
        if (Input.IsActionPressed("fire") && canFire)
        {
            Vector2 fireFromPos = new Vector2(0, 0);
            // Multishot
            float shots = Multishot.GetDynamicVal();
            while (shots > 0)
            {
                if (GD.RandRange(0f, 1) <= shots)
                {
                    ShootBullet(fireFromPos);
                }
                fireFromPos += new Vector2(10 + (GD.Randi() % 5), 0).Rotated(reticule.Rotation) * (GD.Randi() % 2 == 0 ? 1 : -1);
                shots--;
            }

        }

        ((IAffectedByVisualEffects)this).ProcessVisualEffects((float)delta);



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
        Hide();
    }

    public int CurrentHP()
    {
        return currentHP;
    }

    private async void OnBodyEntered(Node2D body)
    {
        // GD.Print("ouch!");
        currentHP--;
        hud.UpdateHealth(currentHP);
        // tell mobs to back off
        foreach (Mob mob in GetTree().GetNodesInGroup("mobs"))
        {
            mob.Recoil(GlobalPosition);
        }
        if (currentHP <= 0)
        {

            Die();
            return;
        }

        // Activate I-frames
        // Need to use deferred because this is called on a physics callback, and can't edit physics properties in a physics callback
        ToggleCollision(false);
        float iframes = 0.5f;
        ((IAffectedByVisualEffects)this).AddVisualEffect(new StaticColourChange(State.MobDamage, Colors.Red, 0.8f, 100, iframes));
        await ToSignal(GetTree().CreateTimer(iframes), SceneTreeTimer.SignalName.Timeout);
        ToggleCollision(true);


    }



    public void Start(Vector2 position)
    {
        UpdateStats();
        Position = position;
        Show();
        ToggleCollision(true);
        bubbleEmitter.Amount = bubbleEmitter.Amount; // Doing this clears existing particles

        ((IAffectedByVisualEffects)this).ImmediateVisualEffects();
    }

    void ToggleCollision(bool toggle)
    {
        GetNode<CollisionShape2D>("BodyCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, !toggle);
        GetNode<CollisionShape2D>("PropellerCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, !toggle);
        GetNode<CollisionPolygon2D>("FinCollision").SetDeferred(CollisionShape2D.PropertyName.Disabled, !toggle);
    }


}
