using Godot;
using System;

public partial class Bomber : OrganicMob
{
    Node2D spawner;

    bool direction;

    float speed = 200;

    public override void _Ready()
    {
        base._Ready();
        spawner = GetNode<Node2D>("Spawner");
        direction = GD.Randf() > 0.5;
    }



    protected override void InitialMovement()
    {
        LookAt(GetViewportRect().GetCenter());
        Rotate(Mathf.Pi / 2);
        ApplyCentralImpulse(Vector2.Up.Rotated(Transform.Rotation) * speed);
    }



    protected override void ProcessMovement(double delta)
    {
        ApplyCentralForce(Vector2.Left.Rotated(Transform.Rotation) * (direction ? 1 : -1) * (float)delta * 100);
        Rotation = Vector2.Up.AngleTo(LinearVelocity);
        if (LinearVelocity.LengthSquared() < speed * speed)
        {
            ApplyCentralForce(LinearVelocity.Normalized() * (speed - LinearVelocity.Length()) * (float)delta * 1000);
        }
    }
}
