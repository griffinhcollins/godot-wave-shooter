using Godot;
using System;

public partial class Player : Area2D
{

	[Signal]
	public delegate void HitEventHandler();

	[Export]
	public int speed { get; set; } = 400; // player movement in pixels/sec

	public Vector2 screenSize; // pixel screen size

	AnimatedSprite2D animSprite;
	CollisionShape2D collShape;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		screenSize = GetViewportRect().Size;
		animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		collShape = GetNode<CollisionShape2D>("CollisionShape2D");
		// Hide();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
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
		// Activate I-frames
		collShape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
	}

	public void Start(Vector2 position)
	{
		Position = position;
		Show();
		collShape.Disabled = false;
	}



}
