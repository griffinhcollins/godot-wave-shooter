using Godot;
using System;

public partial class Bomber : OrganicMob
{

    [Export]
    PackedScene bomb;


    Node2D spawner;

    bool direction;

    float speed = 300;

    public override void _Ready()
    {
        spawner = GetNode<Node2D>("Spawner");
        direction = GD.Randf() > 0.5;
        base._Ready();
    }



    protected override void InitialMovement()
    {
        LookAt(GetViewportRect().GetCenter() + Vector2.Left.Rotated(Rotation) * (direction ? -1 : 1) * 100);
        Rotate(Mathf.Pi / 2);
        ApplyCentralImpulse(Vector2.Up.Rotated(Rotation) * speed);
    }



    protected override void ProcessMovement(double delta)
    {
        Rotation = Vector2.Up.AngleTo(LinearVelocity);
        if (LinearVelocity.LengthSquared() < speed * speed)
        {
            ApplyCentralForce(LinearVelocity.Normalized() * (speed - LinearVelocity.Length()) * (float)delta * 1000);
        }
        ApplyCentralForce(Vector2.Left.Rotated(Rotation) * (direction ? 1 : -1) * (float)delta * 100000);
    }

    void BombTimeout()
    {
        if (dead)
        {
            return;
        }
        // Drop a bomb!
        Bomb newBomb = bomb.Instantiate<Bomb>();
        GetTree().Root.AddChild(newBomb);
        newBomb.SetSize(size * new Vector2(1, 1));
        newBomb.GlobalPosition = spawner.GlobalPosition;
    }

    protected override void Pause()
    {
        base.Pause();
        GetNode<Timer>("BombTimer").Paused = true;
    }

    protected override void UnPause()
    {
        base.UnPause();
        GetNode<Timer>("BombTimer").Paused = false;
    }

}
