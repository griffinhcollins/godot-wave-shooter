using Godot;
using static Stats.PlayerStats.Unlocks;
using System;
using System.Runtime.Serialization;

public partial class PlagueCloud : Area2D
{
    float timeAlive;
    float lifeTime;
    Sprite2D sprite;
    public void Appear()
    {
        lifeTime = plagueCloudLifetime.GetDynamicVal();
        Scale = Vector2.One * plagueExplosionRadius.GetDynamicVal() / 40f;
        sprite = GetNode<Sprite2D>("Sprite2D");
        timeAlive = 0;
    }

    public override void _Process(double delta)
    {
        timeAlive += (float)delta;
        if (timeAlive > lifeTime)
        {
            QueueFree();
        }
        else
        {
            Color currentModulate = sprite.Modulate;
            sprite.Modulate = new Color(currentModulate.R, currentModulate.G, currentModulate.B, (lifeTime - timeAlive) / lifeTime);
        }

    }

    void OnHit(Node2D hit)
    {
        if (hit is Mob)
        {
            ((Mob)hit).Poison();
        }
    }
}
