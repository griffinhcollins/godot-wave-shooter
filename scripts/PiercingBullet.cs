using Godot;
using static Stats;
using System;
using System.Collections.Generic;

public partial class PiercingBullet : Bullet
{

    public Area2D parent;
    public Vector2 velocity;

    public override void _Ready()
    {
        base._Ready();
        parent = GetParent<Area2D>();
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        parent.Position += velocity * (float)delta;
    }


    protected override void HandleCollision()
    {
        return;
    }

}