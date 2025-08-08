using Godot;

using static Stats.EnemyStats;
using static Stats.PlayerStats.Unlocks;
public partial class Drifter : Mob
{



    float offScreenTime = -1;

    public override void _Ready()
    {
        base._Ready();
        InitialMovement();
    }
    // No acceleration towards player while on screen, moves only through momentum
    protected override void ProcessMovement(double delta)
    {
        ScreenCheck((float)delta);
    }


    protected override void InitialMovement()
    {
        float speed = 1 / size;
        ApplyImpulse((GD.Randf() * 0.5f + 0.5f) * 100 * (GetViewportRect().GetCenter() - Position).Normalized() * speed * DynamicStats[ID.AccelerationMult]);

    }


    protected void ScreenCheck(float delta)
    {
        // Once the drifter enters the screen for the first time, next time it leaves the screen it despawns after 1 second
        if (onScreenNotifier2D.IsOnScreen())
        {
            offScreenTime = 1;
            return;

        }
        else
        {
            if (offScreenTime == -1)
            {
                return;
            }
            else
            {
                offScreenTime -= delta;
                if (offScreenTime < 0)
                {
                    QueueFree();
                }
            }
        }
    }


    // Rocks are immune to poison, resistant to sharp and fire damage
    protected override float DamageResistanceMult(DamageType t)
    {
        float parentVal = base.DamageResistanceMult(t);
        if (t == DamageTypes.Sharp || t == DamageTypes.Fire)
        {
            return 0.5f * parentVal;
        }
        if (t == DamageTypes.Poison)
        {
            return 0;
        }

        return parentVal;
    }




    protected override void SetSize()
    {
        size = 0.3f * (GD.Randf() * 0.5f * DynamicStats[ID.SizeMult] + 1);
    }

    protected override void SetScale()
    {
        animSprite.SpeedScale = 1 / (size);
        hp = DynamicStats[ID.HPMult] * GetBaseHealth() * size * 2f;
        animSprite.Scale *= size;
        GetNode<CollisionPolygon2D>("Collider").Scale *= size;
    }

    public override Vector2 GetIndicatorSize()
    {
        return Vector2.One * size * 4;
    }

    // Drifters can't be poisoned
    public override void Poison()
    {
        return;
    }


    public override void Recoil(Vector2 recoilFrom, float mult = 100)
    {
        base.Recoil(recoilFrom, 20);
    }

    protected override float GetBaseHealth()
    {
        return 40;
    }
}