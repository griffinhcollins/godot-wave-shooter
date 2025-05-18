using Godot;
using static Stats.PlayerStats;
using System;

public partial class LaserBeam : Bullet
{

	GpuParticles2D particles;
	CollisionShape2D hitbox;

	Node2D lastHit;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		Node2D parent = GetParent<Node2D>();
		particles = parent.GetNode<GpuParticles2D>("BeamParticles");
		hitbox = parent.GetNode<CollisionShape2D>("CollisionShape2D");

		SetSize();


	}

	void SetSize()
	{

		Vector2 size = Unlocks.LaserSizeVector() * (isShard ? Unlocks.splinterDamageMultiplier.GetDynamicVal() : 1);

		// Set size of collider
		RectangleShape2D collisionShape = new RectangleShape2D();
		collisionShape.Size = size;
		hitbox.Shape = collisionShape;

		// Set size of particles
		particles.ProcessMaterial.Set(ParticleProcessMaterial.PropertyName.EmissionShapeOffset, new Vector3(0, -1 * size.Y, 0));
		particles.ProcessMaterial.Set(ParticleProcessMaterial.PropertyName.EmissionBoxExtents, new Vector3(size.X, size.Y, 1));

		// Set amount of particles to scale with area of laser
		particles.AmountRatio = (1 / 16000f) * (size.X * size.Y);
		GD.Print(size.X);
		GD.Print(size.Y);
		GD.Print(1 / 16000f * (size.X * size.Y));
		GD.Print(particles.AmountRatio);

	}


	private void OnHit(Node2D body)
	{
		if (body.IsInGroup("mobs"))
		{
			Mob mobHit = (Mob)body;
			lastHit = mobHit;
			OnHit(mobHit);
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);

		// Follow the mouse
		GetParent<Node2D>().GlobalRotation = Vector2.Up.AngleTo(GetGlobalMousePosition() - GetParent<Node2D>().GlobalPosition);


		// Introduce a bit of randomness with how long lasers last makes them look a bit less jank with multishot
		if (timeAlive >= Unlocks.LaserLifetime.GetDynamicVal() && GD.Randi() % 3 == 0)
		{
			particles.Hide();
			HandleDeath(lastHit);
		}
	}

	protected override Vector2 GetCurrentVelocity()
	{
		return Vector2.Up.Rotated(GetParent<Node2D>().Rotation);
	}

	protected override void Pause()
	{
		return;
	}

	protected override void UnPause()
	{
		return;
	}

	protected override void SetVelocity(Vector2 newVelocity)
	{
		Rotation = Vector2.Up.AngleTo(newVelocity);
	}
}
