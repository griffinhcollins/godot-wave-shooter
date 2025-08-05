using Godot;
using static Stats.EnemyStats;

public partial class Biter : Organic
{

    float timeAlive;

    float warningTime = 3f;

    Line2D warningLine;

    public override void _Ready()
    {
        base._Ready();
        // Start timer
        timeAlive = 0;

        // Look at the player
        LookAt(player.GlobalPosition);
        Rotate(Mathf.Pi / 2);

        warningLine = GetNode<Line2D>("WarningLine");

    }
    protected override void ProcessMovement(double delta)
    {
        timeAlive += (float)delta;
        if (timeAlive < warningTime)
        {
            warningLine.Width = Mathf.Lerp(0, 30, timeAlive / warningTime);
        }
        else
        {
            warningLine.Hide();
            ApplyCentralForce(Vector2.Up.Rotated(Transform.Rotation) * 1000000 * (float)delta);
        }

    }

    protected override void SetSize()
    {
        size = Mathf.Pow(GD.Randf() * 0.5f, 2) * DynamicStats[ID.SizeMult] + 1;
    }








}