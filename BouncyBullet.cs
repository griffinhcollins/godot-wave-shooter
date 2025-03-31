using Godot;
using static Stats;
using System;
using System.Collections.Generic;

public partial class BouncyBullet : Bullet
{
	protected override void HandleCollision()
	{
		RigidBody2D myRigidbody = GetParent<RigidBody2D>();
		myRigidbody.LinearVelocity = myRigidbody.LinearVelocity.Normalized() * 1000 * PlayerStats.DynamicStats[PlayerStats.ID.ShotSpeed];
	}
}
