using Godot;
using System;

public partial class OffscreenIndicator : Node2D
{

    Mob trackedMob;
    VisibleOnScreenNotifier2D screenNotifier;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // hidden in inspector because otherwise it's annoying
        Show();
        trackedMob = GetParent<Mob>();
        screenNotifier = trackedMob.GetNode<VisibleOnScreenNotifier2D>("VisibleOnScreenNotifier2D");
        Scale = trackedMob.GetIndicatorSize();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

        if (screenNotifier.IsOnScreen())
        {
            Hide();
        }
        else
        {
            Show();
            Vector2 screenSize = GetViewportRect().Size;
            Position = new Vector2(Mathf.Clamp(trackedMob.Position.X, 0, screenSize.X), Mathf.Clamp(trackedMob.Position.Y, 0, screenSize.Y));
            Scale = Vector2.One * 1 / ((trackedMob.Position - Position).LengthSquared() * 0.0001f + 1);
        }

    }
}
