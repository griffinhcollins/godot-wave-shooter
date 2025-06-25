using Godot;
using System;

public partial class Coin : Area2D
{



    int value = 1;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        AnimatedSprite2D sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        sprite.Animation = "default";
        sprite.Frame = GD.RandRange(0, 4);
        sprite.Play();
        GetNode<Timer>("ExpireTimer").Start();
    }

    private void Expire(){
        QueueFree();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


    private void OnTouchPlayer(Node2D touchedArea)
    {
        Stats.Counters.CoinCounter.Value++;
        Player player;
        if (touchedArea.IsInGroup("player"))
        {
            player = (Player)touchedArea;
        }
        else
        {
            return;
        }

        AudioStreamPlayer soundEffect = GetNode<AudioStreamPlayer>("PickupNoise");
        soundEffect.Play();
        player.AddMoney(value);
        Hide();
        CollisionLayer = 0; // Disable collision
    }

    private void SoundFinished()
    {
        QueueFree();
    }

}
