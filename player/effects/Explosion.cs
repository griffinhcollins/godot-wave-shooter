using System.Collections.Generic;
using Godot;
using static Stats.PlayerStats.Unlocks;

public partial class Explosion : Area2D
{
    float timeAlive;
    float lifeTime;
    Sprite2D sprite;

    HashSet<Mob> hits;

    public void Appear()
    {
        hits = new();
        sprite = GetNode<Sprite2D>("Sprite2D");
        timeAlive = 0;
        Initialize();
    }

    protected virtual void Initialize()
    {
        lifeTime = 0.3f;
        Scale = Vector2.One * explosionRadius.GetDynamicVal() / 40f;
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

    protected virtual void OnHit(Node2D hit)
    {
        if (hit is Mob && !hits.Contains((Mob)hit))
        {
            hits.Add((Mob)hit);
            ((Mob)hit).TakeDamage(explosionDamage.GetDynamicVal());
        }
    }
}
