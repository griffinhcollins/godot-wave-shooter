using Godot;
using static Stats.PlayerStats;
using System;

public partial class LaserBeam : Bullet
{

	GpuParticles2D particles;
	CollisionShape2D hitbox;



	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		Node2D parent = GetParent<Node2D>();
		particles = parent.GetNode<GpuParticles2D>("BeamParticles");
		hitbox = parent.GetNode<CollisionShape2D>("CollisionShape2D");

		RectangleShape2D collisionShape = new RectangleShape2D();
		collisionShape.Size = new Vector2(20 * BulletSize.GetDynamicVal(), 400 * ShotSpeed.GetDynamicVal());

		hitbox.Shape = collisionShape;


	}


	private void OnHit(Node2D body)
	{
		if (body.IsInGroup("mobs"))
		{
			Mob mobHit = (Mob)body;
			mobHit.TakeDamage(Damage.GetDynamicVal());
		}
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		base._Process(delta);
		// Introduce a bit of randomness with how long lasers last makes them look a bit less jank with multishot
		if (timeAlive >= Unlocks.LaserLifetime.GetDynamicVal() && GD.Randi() % 3 == 0)
		{
			particles.Hide();
			HandleDeath();
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
