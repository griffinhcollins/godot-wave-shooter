using Godot;
using System;

public partial class Coin : Area2D
{



    int value = 1;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Animation = "default";
        GetNode<AnimatedSprite2D>("AnimatedSprite2D").Play();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


    private void OnTouchPlayer(Area2D touchedArea)
    {
        Player player;
        if (touchedArea.IsInGroup("player"))
        {
            player = (Player)touchedArea;
        }
        else
        {
            return;
        }

        player.AddMoney(value);
        QueueFree();

    }

}
