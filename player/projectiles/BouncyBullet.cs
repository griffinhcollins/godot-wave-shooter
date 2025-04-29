using Godot;
using static Stats;
using System;
using System.Collections.Generic;

public partial class BouncyBullet : Bullet
{

	RigidBody2D parent;

	public override void _Ready()
	{
		base._Ready();

		parent = GetParent<RigidBody2D>();
		
		parent.GetNode<CollisionShape2D>("CollisionShape2D").Scale = Vector2.One * PlayerStats.BulletSize.GetDynamicVal();
		parent.GetNode<Sprite2D>("Sprite2D").Scale = Vector2.One * PlayerStats.BulletSize.GetDynamicVal();
		
	}

	protected override void HandleCollision()
	{
		parent.LinearVelocity = parent.LinearVelocity.Normalized() * 1000 * PlayerStats.ShotSpeed.GetDynamicVal();
	}
}
