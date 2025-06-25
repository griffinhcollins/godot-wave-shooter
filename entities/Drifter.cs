using Godot;

using static Stats.EnemyStats;
using static Stats.PlayerStats.Unlocks;
public partial class Drifter : Mob
{

    VisibleOnScreenNotifier2D onScreenNotifier;

    public override void _Ready()
    {
        base._Ready();
        onScreenNotifier = GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        ApplyImpulse(0.05f * GetViewportRect().GetCenter() - Position);
    }
    // No acceleration towards player while on screen, moves only through momentum
    protected override void ProcessMovement(double delta)
    {
        if (onScreenNotifier.IsOnScreen())
        {
            return;

        }
        else
        {
            // return to the screen
            Vector2 toCentreOfScreen = GetViewportRect().GetCenter() - GlobalPosition;
            GD.Print(toCentreOfScreen);
            // we want it to be like gravity, so divide by distance squared. since the vector is already multiplied by distance, divide by distance cubed
            float distSq = toCentreOfScreen.LengthSquared();
            ApplyForce((float)delta * toCentreOfScreen * 1000);
        }
    }



    protected override void SetScale()
    {
        GetNode<CollisionPolygon2D>("Collider").Scale *= size;
    }

    public override void Recoil(Vector2 recoilFrom, float mult = 100)
    {
        base.Recoil(recoilFrom, 10);
    }
}