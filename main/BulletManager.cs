using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BulletManager : Node2D
{

	Rid sharedArea;

	List<BulletStructure> bullets;

	[Export]
	Texture2D bulletImage;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		State.bulletManager = this;
		bullets = new List<BulletStructure>();
		sharedArea = PhysicsServer2D.AreaCreate();
		Transform2D areaTransform = new Transform2D(0, new Vector2(1280 / 2, 720 / 2));
		PhysicsServer2D.AreaSetTransform(sharedArea, areaTransform);
		PhysicsServer2D.AreaSetCollisionLayer(sharedArea, 3);
		PhysicsServer2D.AreaSetCollisionMask(sharedArea, 2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Draw()
	{
		Vector2 offset = bulletImage.GetSize() / 2;
		foreach (BulletStructure b in bullets)
		{
			DrawTexture(bulletImage, b.position - offset);
		}
    }


	public override void _PhysicsProcess(double delta)
	{
		List<BulletStructure> tooOld = new List<BulletStructure>();
		Transform2D t = new Transform2D(0, Vector2.One);
		for (int i = 0; i < bullets.Count; i++)
		{

			BulletStructure b = bullets[i];

			b.lifetime += (float)delta;

			if (b.lifetime > Stats.PlayerStats.Lifetime.GetDynamicVal())
			{
				tooOld.Add(b);
				continue;
			}

			Vector2 offset = b.direction * b.speed * (float)delta;
			b.position += offset;
			t.Origin = b.position;
			PhysicsServer2D.AreaSetShapeTransform(sharedArea, i, t);
			GD.Print(b.position);
		}
		for (int i = 0; i < tooOld.Count; i++)
		{
			BulletStructure bToKill = tooOld[i];
			PhysicsServer2D.FreeRid(bToKill.shapeID);
			bullets.Remove(bToKill);
		}
		QueueRedraw();
	}



	public void SpawnBullet(Vector2 direction, float speed, Vector2 initialPosition)
	{
		GD.Print("spawned a bullet");
		BulletStructure newBullet = new BulletStructure();
		newBullet.direction = direction;
		newBullet.position = initialPosition;
		newBullet.speed = speed;
		ConfigureCollision(newBullet);
		bullets.Add(newBullet);
	}


	private void ConfigureCollision(BulletStructure bullet)
	{
		Transform2D bulletTransform = new Transform2D(0, Position);
		bulletTransform.Origin = bullet.position;
		Rid circle = PhysicsServer2D.CircleShapeCreate();
		PhysicsServer2D.ShapeSetData(circle, 15);
		PhysicsServer2D.AreaAddShape(sharedArea, circle, bulletTransform);
		bullet.shapeID = circle;
	}

}
