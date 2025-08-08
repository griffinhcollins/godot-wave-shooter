using Godot;
using System;

public partial class Bomb : RigidBody2D
{

    float timeAlive;

    public override void _Ready()
    {
        timeAlive = 0;
    }

    public override void _Process(double delta)
    {
        if (State.currentState == State.paused)
        {
            return;
        }
        timeAlive += (float)delta;
        if (timeAlive > 2)
        {
            QueueFree();
        }
    }

    public void SetSize(Vector2 scale)
    {
        GetNode<Sprite2D>("MainSprite").Scale = scale * 0.05f;
        GetNode<CollisionShape2D>("Collider").Scale = scale;
    }


}
