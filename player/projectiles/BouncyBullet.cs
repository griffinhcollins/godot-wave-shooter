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

    public override Vector2 GetCurrentVelocity()
    {
        return parent.LinearVelocity;
    }

    protected override void HandleCollision(Node2D hitNode)
    {
        parent.SetDeferred(RigidBody2D.PropertyName.LinearVelocity, parent.LinearVelocity.Normalized() * 1000 * PlayerStats.ShotSpeed.GetDynamicVal());
        base.HandleCollision(hitNode);

    }

    protected override void Pause()
    {
        parent.LinearVelocity = Vector2.Zero;
    }

    public override void SetVelocity(Vector2 newVelocity, bool normalize = true)
    {
        base.SetVelocity(newVelocity);
        if (parent is null)
        {
            parent = GetParent<RigidBody2D>();
        }
        if (normalize)
        {
            parent.SetDeferred(RigidBody2D.PropertyName.LinearVelocity, newVelocity.Normalized() * 1000 * PlayerStats.ShotSpeed.GetDynamicVal());

        }
        else
        {
            parent.SetDeferred(RigidBody2D.PropertyName.LinearVelocity, newVelocity);
        }
    }

    protected override void UnPause()
    {
        parent.LinearVelocity = beforePauseVelocity;
    }

}
