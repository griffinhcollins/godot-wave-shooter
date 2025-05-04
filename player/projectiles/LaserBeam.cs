using Godot;
using static Stats.PlayerStats;
using System;

public partial class LaserBeam : Area2D
{

	GpuParticles2D particles;
	CollisionShape2D hitbox;

	double lifeLeft;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		particles = GetNode<GpuParticles2D>("BeamParticles");
		hitbox = GetNode<CollisionShape2D>("CollisionShape2D");

		RectangleShape2D collisionShape = new RectangleShape2D();
		collisionShape.Size = new Vector2(20, 400 * ShotSpeed.GetDynamicVal());

		hitbox.Shape = collisionShape;

		lifeLeft = Unlocks.LaserLifetime.GetDynamicVal();

	}


	private void OnHit(Node2D body){
		if (body.IsInGroup("mobs"))
        {
            Mob mobHit = (Mob)body;
            mobHit.TakeDamage(Damage.GetDynamicVal());
        }
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	
		lifeLeft -= delta;
		// Introduce a bit of randomness with how long lasers last makes them look a bit less jank with multishot
		if (lifeLeft <= 0 && GD.Randi() % 3 == 0)
		{
			particles.Hide();
			QueueFree();
		}
	}
}
