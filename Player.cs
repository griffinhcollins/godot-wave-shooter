using Godot;
using System;
using System.Runtime.ExceptionServices;

public partial class Player : Area2D
{

    [Signal]
    public delegate void KilledEventHandler();

    [Export]
    public PackedScene bullet;

    float damage; // Default damage per shot
    float firingspeed; // Shots/second
    int hp;
    int money;

    public int speed { get; set; } = 400; // player movement in pixels/sec

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
        damage = Stats.Player.BaseDamage;
        firingspeed = Stats.Player.FiringSpeed;
        bulletTimer.WaitTime = 1 / firingspeed;
        hp = Stats.Player.HP;
        hud.UpdateHealth(hp);
    }


    public void AddMoney(int amount)
    {
        money += amount;
        hud.UpdateMoneyCounter(money);
    }

    private void OnBulletTimerFinished()
    {
        canFire = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        // Point reticule at mouse
        Vector2 pointVec = GetGlobalMousePosition() - Position;

        reticule.Rotation = -1 * pointVec.AngleTo(Vector2.Up);

        // Fire if clicking
        if (Input.IsMouseButtonPressed(MouseButton.Left) && canFire)
        {
            canFire = false;
            bulletTimer.Start();
            RigidBody2D newBullet = bullet.Instantiate<RigidBody2D>();
            newBullet.LinearVelocity = new Vector2(0, -1000).Rotated(reticule.Rotation);
            AddChild(newBullet);
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
            velocity = velocity.Normalized() * speed;
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

    private async void OnBodyEntered(Node2D body)
    {
        // GD.Print("ouch!");
        hp--;
        hud.UpdateHealth(hp);
        if (hp <= 0)
        {

            EmitSignal(SignalName.Killed);
            Hide();
            return;
        }
        // Activate I-frames
        // Need to use deferred because this is called on a physics callback, and can't edit physics properties in a physics callback
        collShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
        sprite.Modulate = Color.Color8(255, 0, 0);
        await ToSignal(GetTree().CreateTimer(0.3f), SceneTreeTimer.SignalName.Timeout);
        collShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
        sprite.Modulate = Color.Color8(255, 255, 255);


    }

    public void Start(Vector2 position)
    {
        money = 0;
        UpdateStats();
        Position = position;
        Show();
        collShape.Disabled = false;
    }



}
