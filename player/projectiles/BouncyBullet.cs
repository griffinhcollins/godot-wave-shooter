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
		
		
		
	}

    protected override Vector2 GetCurrentVelocity()
    {
        return parent.LinearVelocity;
    }

    protected override void HandleCollision()
	{
		parent.LinearVelocity = parent.LinearVelocity.Normalized() * 1000 * PlayerStats.ShotSpeed.GetDynamicVal();
	}

    protected override void Pause()
    {
        parent.LinearVelocity = Vector2.Zero;
    }

	protected override void UnPause()
    {
        parent.LinearVelocity = beforePauseVelocity;
    }

}
