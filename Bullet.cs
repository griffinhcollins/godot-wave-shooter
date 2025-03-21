using Godot;
using System;

public partial class Bullet : RigidBody2D
{

    float dmg;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        SetDamage(Stats.Player.Damage);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    public void SetDamage(float newDmg)
    {
        dmg = newDmg;
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
