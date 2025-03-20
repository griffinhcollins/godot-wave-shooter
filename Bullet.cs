using Godot;
using System;

public partial class Bullet : RigidBody2D
{

    float dmg = 5;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


    private void OnCollision(Node2D body)
    {
        if (body.IsInGroup("mobs"))
        {
            Mob mobHit = (Mob)body;
            mobHit.TakeDamage(dmg);
        }
        Hide();
        QueueFree();
    }



}
