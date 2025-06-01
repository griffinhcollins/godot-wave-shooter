using Godot;
using static Stats.PlayerStats.Unlocks;
using System;
using System.Runtime.Serialization;

public partial class PlagueCloud : Explosion
{
    float timeAlive;
    float lifeTime;
    Sprite2D sprite;

    protected override void Initialize()
    {
        lifeTime = plagueCloudLifetime.GetDynamicVal();
        Scale = Vector2.One * plagueExplosionRadius.GetDynamicVal() / 40f;
    }



    protected override void OnHit(Node2D hit)
    {
        if (hit is Mob)
        {
            ((Mob)hit).Poison();
            ((Mob)hit).SetExplodeOnDeath(true);
        }
    }
}
