using Godot;
using Godot.Collections;
using static Stats.PlayerStats;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Contracts;
using System.Diagnostics;

public partial class BulletManager : Node2D
{



	Rid sharedArea;

	Rid bulletShape;

	List<Bullet> bullets;
	GpuParticles2D instantiatedLaserParticles;

	// Laser information
	bool firingLaser = false;
	List<LaserInformation> lastLaserInformation;

	[Export]
	Texture2D bouncyBulletTexture;
	[Export]
	Texture2D piercingBulletTexture;
	[Export]
	Texture2D laserTexture;
	[Export]
	PackedScene laserParticles;

	public class LaserInformation
	{
		public Vector2 _source;
		public Vector2 _target;
		public float _length;
		public LaserInformation(Vector2 source, Vector2 target)
		{
			_source = source;
			_target = target;
		}

		public float GetLength()
		{
			if (_length == 0)
			{
				_length = (_source - _target).Length();
			}
			return _length;
		}

	}

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
		lastLaserInformation = new List<LaserInformation>();

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
		if (Unlocks.Laser.unlocked)
		{
			return laserTexture;
		}
		if (Unlocks.PiercingBullets.unlocked)
		{
			return piercingBulletTexture;
		}
		return bouncyBulletTexture;
	}

	public override void _Draw()
	{
		Vector2 offset = GetTexture().GetSize() / 2;
		if (Unlocks.Laser.unlocked)
		{
			if (firingLaser)
			{
				foreach (LaserInformation info in lastLaserInformation)
				{
					DrawLine(info._source, info._target, Colors.White, 20);
				}
			}
			else if (instantiatedLaserParticles is not null)
			{
				// instantiatedLaserParticles.Hide();

			}

		}
		else
		{
			foreach (Bullet b in bullets)
			{
				// GD.Print(b.position);
				StaticColourChange staticColour = ((IAffectedByVisualEffects)b).GetStaticColour();
				DrawSetTransform(b.position, Vector2.Up.AngleTo(b.direction), b.GetScale() * Vector2.One);
				DrawTexture(GetTexture(), -offset, staticColour is null ? Colors.White : staticColour.modulate);
			}
		}


	}


	public override void _PhysicsProcess(double delta)
	{
		List<Bullet> tooOld = new List<Bullet>();
		Transform2D t = new Transform2D(0, Vector2.One);
		if (Unlocks.Laser.unlocked)
		{
			// GD.Print(firingLaser);
			if (firingLaser)
			{

				// laser!


			}
			else if (instantiatedLaserParticles is not null)
			{
				// instantiatedLaserParticles.Hide();
			}
		}
		else
		{
			// regular bullets
			for (int i = 0; i < bullets.Count; i++)
			{
				Bullet b = bullets[i];

				b.lifetime += (float)delta;

				if (b.lifetime > Lifetime.GetDynamicVal())
				{
					tooOld.Add(b);
					continue;
				}
				GD.Print(PhysicsServer2D.AreaGetShapeTransform(sharedArea, i).Rotation);
				PhysicsServer2D.AreaSetShapeTransform(sharedArea, i, PhysicsServer2D.AreaGetShapeTransform(sharedArea, i).Rotated(Vector2.Down.AngleTo(b.direction)));
				GD.Print(PhysicsServer2D.AreaGetShapeTransform(sharedArea, i).Rotation);
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



	public Bullet SpawnBullet(Vector2 direction, Vector2 initialPosition, float speed = -1, bool isShard = false)
	{
		if (speed == -1)
		{
			speed = ShotSpeed.GetDynamicVal() * 1000;
		}
		// GD.Print("spawned a bullet");
		Bullet newBullet = new Bullet();
		newBullet.isShard = isShard;
		newBullet.direction = direction;
		newBullet.position = initialPosition;
		newBullet.speed = speed;
		bullets.Add(newBullet);
		ConfigureCollision(newBullet);
		newBullet.Initialize(); // Populates instantiatedParticles
		foreach (GpuParticles2D p in newBullet.instantiatedParticles.Values)
		{
			AddChild(p);
			p.GlobalPosition = initialPosition;
		}
		return newBullet;
	}

	public void FireLaser(Vector2 direction, Vector2 initialPosition, float hitDist, int multishotIndex = 0, bool isShard = false)
	{
		// if (instantiatedLaserParticles is null)
		// {
		// 	instantiatedLaserParticles = laserParticles.Instantiate<GpuParticles2D>();
		// 	Vector2 size = Unlocks.LaserSizeVector();
		// 	instantiatedLaserParticles.ProcessMaterial.Set(ParticleProcessMaterial.PropertyName.EmissionShapeOffset, new Vector3(0, -1 * size.Y, 0));
		// 	instantiatedLaserParticles.ProcessMaterial.Set(ParticleProcessMaterial.PropertyName.EmissionBoxExtents, new Vector3(size.X, size.Y, 1));
		// 	AddChild(instantiatedLaserParticles);
		// }


		LaserInformation newInfo = new LaserInformation(initialPosition, initialPosition + direction * hitDist);
		if (lastLaserInformation.Count <= multishotIndex)
		{
			GD.Print(multishotIndex);
			lastLaserInformation.Add(newInfo);
		}
		lastLaserInformation[multishotIndex] = newInfo;
		firingLaser = true;
		QueueRedraw();
	}

	public void StopLaser()
	{
		firingLaser = false;
		QueueRedraw();
	}

	public void CheckLaser(Bullet laser)
	{

		PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
		Dictionary rayCast = spaceState.IntersectRay(PhysicsRayQueryParameters2D.Create(laser.position, laser.position + laser.direction * 200 * ShotSpeed.GetDynamicVal()));
		if (rayCast.Count > 0)
		{
			laser.OnCollision((Node2D)rayCast["collider"]);
		}
	}




	private void ConfigureCollision(Bullet bullet)
	{
		if (bullet.isLaser)
		{
			// Vector2 sizeVector = Unlocks.LaserSizeVector();
			// Transform2D bulletTransform = new Transform2D(Vector2.Down.AngleTo(bullet.direction), Vector2.One * 6000 + bullet.position + bullet.direction * sizeVector.Y / 2f);
			// Rid rectangle = PhysicsServer2D.RectangleShapeCreate();
			// PhysicsServer2D.ShapeSetData(rectangle, sizeVector);
			// PhysicsServer2D.AreaAddShape(sharedArea, rectangle, bulletTransform);
			// bullet.shapeID = rectangle;
		}
		else
		{
			Transform2D bulletTransform = new Transform2D(0, bullet.position);
			bulletTransform.Origin = bullet.position;
			Rid circle = PhysicsServer2D.CircleShapeCreate();
			PhysicsServer2D.ShapeSetData(circle, bullet.GetScale() * 15);
			PhysicsServer2D.AreaAddShape(sharedArea, circle, bulletTransform);
			bullet.shapeID = circle;
		}

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
