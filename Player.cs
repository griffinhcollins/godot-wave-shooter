using Godot;
using static Stats.PlayerStats;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

public partial class Player : Area2D
{

    [Signal]
    public delegate void KilledEventHandler();

    [Export]
    public PackedScene bounceBullet;
    [Export]
    public PackedScene pierceBullet;
    [Export]
    public PackedScene laserBeam;

    Vector2 fireFromPos;
    float damage; // Default damage per shot
    float firingspeed; // Shots/second
    int currentHP;
    int money;


    public Vector2 screenSize; // pixel screen size

    AnimatedSprite2D sprite;
    CollisionShape2D collShape;

    Node2D reticule;

    Timer bulletTimer;

    Hud hud;



    bool canFire = true;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        Hide();
        screenSize = GetViewportRect().Size;
        sprite = GetNode<AnimatedSprite2D>("PlayerSprite");
        collShape = GetNode<CollisionShape2D>("CollisionShape2D");

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
    }


    public void AddMoney(int amount)
    {
        money += amount;
        hud.UpdateMoneyCounter(money);
    }

    public bool ChargeMoney(int amount)
    {
        if (money < amount)
        {
            return false;
        }
        else
        {
            money -= amount;
            hud.UpdateMoneyCounter(money);
            return true;
        }
    }

    private void OnBulletTimerFinished()
    {
        canFire = true;
    }

    private void ShootBullet()
    {

        canFire = false;
        bulletTimer.Start();

        float spreadRotate = reticule.Rotation + Mathf.DegToRad(Spread.GetDynamicVal()) * (float)GD.RandRange(-1, 1f);

        if (Unlocks.DynamicUnlocks[Unlocks.UnlockID.Laser])
        {
            Area2D newBeam = laserBeam.Instantiate<Area2D>();
            newBeam.Position = fireFromPos;
            newBeam.Rotate(spreadRotate);

            AddChild(newBeam);
        }
        else
        {

            // Regular bullet
            bool bouncy = Pierces.GetDynamicVal() <= Bounces.GetDynamicVal();
            Vector2 velocity = new Vector2(0, -1000 * ShotSpeed.GetDynamicVal()).Rotated(spreadRotate);
            if (bouncy)
            {
                // Bouncing, it's a rigidbody
                RigidBody2D newBullet = bounceBullet.Instantiate<RigidBody2D>();
                newBullet.LinearVelocity = velocity;
                newBullet.Position = fireFromPos;
                AddChild(newBullet);
            }
            else
            {
                // Piercing, it's an Area2D
                Area2D newBullet = pierceBullet.Instantiate<Area2D>();
                newBullet.Rotate(spreadRotate);
                PiercingBullet pierceBehaviour = newBullet.GetNode<PiercingBullet>("ScriptHolder");
                pierceBehaviour.velocity = velocity;
                newBullet.Position = Position + fireFromPos;
                AddChild(newBullet);
            }


        }

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        // Point reticule at mouse
        Vector2 pointVec = GetGlobalMousePosition() - Position;

        reticule.Rotation = -1 * pointVec.AngleTo(Vector2.Up);


        // Regular Fire if clicking 
        if (Input.IsActionPressed("fire") && canFire)
        {
            fireFromPos = new Vector2(0, 0);
            // Multishot
            float shots = Multishot.GetDynamicVal();
            while (shots > 0)
            {
                if (GD.RandRange(0f, 1) <= shots)
                {
                    ShootBullet();
                }
                fireFromPos += new Vector2(10 + (GD.Randi() % 5), 0).Rotated(reticule.Rotation) * (GD.Randi() % 2 == 0 ? 1 : -1);
                shots--;
            }

        }





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
        Position += velocity * (float)delta;
        Position = new Vector2(Mathf.Clamp(Position.X, 0, screenSize.X), Mathf.Clamp(Position.Y, 0, screenSize.Y));


        // Update animations
        if (velocity.X != 0)
        {
            sprite.Animation = "walk";
            sprite.FlipV = false;
            sprite.FlipH = velocity.X < 0;
        }
        else if (velocity.Y != 0)
        {
            sprite.Animation = "up";
            sprite.FlipV = velocity.Y > 0;

        }

    }

    private void Die()
    {
        money = 0;
        EmitSignal(SignalName.Killed);
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
        if (currentHP <= 0)
        {

            Die();
            return;
        }
        // Activate I-frames
        // Need to use deferred because this is called on a physics callback, and can't edit physics properties in a physics callback
        collShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        sprite.Modulate = Color.Color8(255, 0, 0);
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
        collShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        sprite.Modulate = Color.Color8(255, 255, 255);


    }

    public void Start(Vector2 position)
    {
        UpdateStats();
        Position = position;
        Show();
        collShape.Disabled = false;
    }



}
