using Godot;
using System;

public partial class Player : Area2D
{

    [Signal]
    public delegate void HitEventHandler();

    [Export]
    public PackedScene bullet;


    public int speed { get; set; } = 400; // player movement in pixels/sec

    public Vector2 screenSize; // pixel screen size

    AnimatedSprite2D animSprite;
    CollisionShape2D collShape;

    Node2D reticule;

    Timer bulletTimer;
    bool canFire = true;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenSize = GetViewportRect().Size;
        animSprite = GetNode<AnimatedSprite2D>("PlayerSprite");
        collShape = GetNode<CollisionShape2D>("CollisionShape2D");

        reticule = GetNode<Node2D>("ReticuleHolder");
        bulletTimer = GetNode<Timer>("BulletTimer");

        // Hide();
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
            animSprite.Play();
        }
        else
        {
            animSprite.Stop();
        }

        // Move
        Position += velocity * (float)delta;
        Position = new Vector2(Mathf.Clamp(Position.X, 0, screenSize.X), Mathf.Clamp(Position.Y, 0, screenSize.Y));


        // Update animations
        if (velocity.X != 0)
        {
            animSprite.Animation = "walk";
            animSprite.FlipV = false;
            animSprite.FlipH = velocity.X < 0;
        }
        else if (velocity.Y != 0)
        {
            animSprite.Animation = "up";
            animSprite.FlipV = velocity.Y > 0;

        }

    }

    private void OnBodyEntered(Node2D body)
    {

        Hide();
        EmitSignal(SignalName.Hit);
        // Activate I-frames, die in one hit but don't want to emit the signal multiple times
        // Need to use deferred because this is called on a physics callback, and can't edit physics properties in a physics callback
        collShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
    }

    public void Start(Vector2 position)
    {
        Position = position;
        Show();
        collShape.Disabled = false;
    }



}
