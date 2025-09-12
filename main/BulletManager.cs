using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BulletManager : Node2D
{



	Rid sharedArea;

	Rid bulletShape;

	List<Bullet> bullets;

	[Export]
	Texture2D bouncyBulletTexture;
	[Export]
	Texture2D piercingBulletTexture;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		bulletShape = PhysicsServer2D.CircleShapeCreate();
		State.bulletManager = this;
		bullets = new List<Bullet>();
		sharedArea = GetNode<Area2D>("Area2D").GetRid();
		Transform2D areaTransform = new Transform2D(0, new Vector2(1280 / 2, 720 / 2));
		PhysicsServer2D.AreaSetTransform(sharedArea, areaTransform);
		PhysicsServer2D.AreaSetCollisionLayer(sharedArea, 3);
		PhysicsServer2D.AreaSetCollisionMask(sharedArea, 2);

	}
	private void BodyShapeEntered(Rid body_rid, Node2D body, int body_shape_index, int local_index)
	{
		bullets[local_index].OnCollision(body);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	Texture2D GetTexture()
	{
		if (Stats.PlayerStats.Unlocks.PiercingBullets.unlocked)
		{
			return piercingBulletTexture;
		}
		return bouncyBulletTexture;
	}

	public override void _Draw()
	{
		Vector2 offset = bouncyBulletTexture.GetSize() / 2;
		foreach (Bullet b in bullets)
		{
			// GD.Print(b.position);
			StaticColourChange staticColour = ((IAffectedByVisualEffects)b).GetStaticColour();
			DrawSetTransform(b.position, Vector2.Up.AngleTo(b.direction));
			DrawTexture(GetTexture(), -offset, staticColour is null ? Colors.White : staticColour.modulate);
		}
	}


	public override void _PhysicsProcess(double delta)
	{
		List<Bullet> tooOld = new List<Bullet>();
		Transform2D t = new Transform2D(0, Vector2.One);
		for (int i = 0; i < bullets.Count; i++)
		{
			Bullet b = bullets[i];

			b.lifetime += (float)delta;

			if (b.lifetime > Stats.PlayerStats.Lifetime.GetDynamicVal())
			{
				tooOld.Add(b);
				continue;
			}

			Vector2 offset = b.direction * b.speed * (float)delta;
			b.position += offset;
			foreach (GpuParticles2D p in b.instantiatedParticles.Values)
			{
				p.Position = b.position;
			}
			t.Origin = b.position;
			PhysicsServer2D.AreaSetShapeTransform(sharedArea, i, t);
		}
		for (int i = 0; i < tooOld.Count; i++)
		{
			Bullet bToKill = tooOld[i];
			bToKill.HandleDeath();
		}
		QueueRedraw();
	}

	public void DestroyBullet(Bullet b)
	{
		PhysicsServer2D.FreeRid(b.shapeID);
		bullets.Remove(b);
		foreach (GpuParticles2D p in b.instantiatedParticles.Values)
		{
			p.QueueFree();
		}
	}



	public Bullet SpawnBullet(Vector2 direction, Vector2 initialPosition, float speed = -1)
	{
		if (speed == -1)
		{
			speed = Stats.PlayerStats.ShotSpeed.GetDynamicVal() * 1000;
		}
		// GD.Print("spawned a bullet");
		Bullet newBullet = new Bullet();
		newBullet.direction = direction;
		newBullet.position = initialPosition;
		newBullet.speed = speed;
		ConfigureCollision(newBullet);
		bullets.Add(newBullet);
		newBullet.Initialize(); // Populates instantiatedParticles
		foreach (GpuParticles2D p in newBullet.instantiatedParticles.Values)
		{
			AddChild(p);
			p.GlobalPosition = initialPosition;
		}
		return newBullet;
	}


	private void ConfigureCollision(Bullet bullet)
	{
		Transform2D bulletTransform = new Transform2D(0, bullet.position);
		bulletTransform.Origin = bullet.position;
		Rid circle = PhysicsServer2D.CircleShapeCreate();
		PhysicsServer2D.ShapeSetData(circle, 15 * Stats.PlayerStats.BulletSize.GetDynamicVal());
		PhysicsServer2D.AreaAddShape(sharedArea, circle, bulletTransform);
		bullet.shapeID = circle;
	}





	// 	public void NewSpawn(Vector2 direction, float speed, Vector2 initialPosition)
	// {
	// 	PhysicsServer2D.ShapeSetData(bulletShape, 15);
	// 	Bullet b = new Bullet();
	// 	b.speed = speed;
	// 	b.direction = direction;
	// 	b.position = initialPosition;
	// 	b.body = PhysicsServer2D.BodyCreate();

	// 	PhysicsServer2D.BodySetSpace(b.body, GetWorld2D().Space);
	// 	PhysicsServer2D.BodySetConstantForce(b.body, Vector2.Zero);
	// 	PhysicsServer2D.BodyAddShape(b.body, bulletShape);

	// 	PhysicsServer2D.BodySetCollisionMask(b.body, 2);
	// 	PhysicsServer2D.BodySetCollisionLayer(b.body, 3);
	// 	Transform2D t = new Transform2D();
	// 	t.Origin = b.position;

	// 	PhysicsServer2D.BodySetState(b.body, PhysicsServer2D.BodyState.Transform, t);
	// 	bullets.Add(b);
	// }

}
