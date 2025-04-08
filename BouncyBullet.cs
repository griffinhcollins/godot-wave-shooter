using Godot;
using static Stats;
using System;
using System.Collections.Generic;

public partial class BouncyBullet : Bullet
{

	public override void _Ready()
	{
		base._Ready();
		if (PlayerStats.Unlocks.WallBounce.unlocked)
		{
			GetParent<RigidBody2D>().SetCollisionMaskValue(5, true);

		}
	}

	protected override void HandleCollision()
	{
		RigidBody2D myRigidbody = GetParent<RigidBody2D>();
		myRigidbody.LinearVelocity = myRigidbody.LinearVelocity.Normalized() * 1000 * PlayerStats.ShotSpeed.GetDynamicVal();
	}
}
