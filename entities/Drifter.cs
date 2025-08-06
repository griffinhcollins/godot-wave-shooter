using Godot;

using static Stats.EnemyStats;
using static Stats.PlayerStats.Unlocks;
public partial class Drifter : Mob
{


    VisibleOnScreenNotifier2D onScreenNotifier;

    float offScreenTime = -1;

    public override void _Ready()
    {
        base._Ready();
        onScreenNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        float speed = 1 / size;
        ApplyImpulse((GD.Randf() * 0.5f + 0.5f) * 200 * (GetViewportRect().GetCenter() - Position).Normalized() * speed * DynamicStats[ID.AccelerationMult]);
    }
    // No acceleration towards player while on screen, moves only through momentum
    protected override void ProcessMovement(double delta)
    {
        if (onScreenNotifier.IsOnScreen())
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
                offScreenTime -= (float)delta;
                if (offScreenTime < 0)
                {
                    QueueFree();
                }
            }
            // // return to the screen
            // Vector2 toCentreOfScreen = GetViewportRect().GetCenter() - GlobalPosition;
            // GD.Print(toCentreOfScreen);
            // // we want it to be like gravity, so divide by distance squared. since the vector is already multiplied by distance, divide by distance cubed
            // float distSq = toCentreOfScreen.LengthSquared();
            // ApplyForce((float)delta * toCentreOfScreen * 1000);
        }
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