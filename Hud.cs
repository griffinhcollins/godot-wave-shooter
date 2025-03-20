using Godot;
using System;

public partial class Hud : CanvasLayer
{
    [Signal]
    public delegate void StartGameEventHandler();


    [Export]
    public PackedScene heart;

    public void UpdateWaveTime(int wavetime)
    {
        GetNode<Label>("WaveLabel").Text = wavetime.ToString();
    }



    public void UpdateHealth(int HP)
    {
        HBoxContainer healthbar = GetNode<HBoxContainer>("HealthBar");
        foreach (TextureRect heart in healthbar.GetChildren())
        {
            heart.QueueFree();
        }
        for (int i = 0; i < HP; i++)
        {
            healthbar.AddChild(heart.Instantiate<TextureRect>());
        }
    }

    public void ShowMessage(string text)
    {
        Label message = GetNode<Label>("Message");
        message.Text = text;
        message.Show();

        GetNode<Timer>("MessageTimer").Start();
    }

    private void OnStartButtonPressed()
    {
        GetNode<Button>("StartButton").Hide();
        EmitSignal(SignalName.StartGame);
    }


    private void OnMessageTimerTimeout()
    {
        GetNode<Label>("Message").Hide();
    }

    public void UpdateMoneyCounter(int money)
    {
        GetNode<Label>("MoneyLabel").Text = money.ToString();
    }

    public void ShowGameOver()
    {
        ShowMessage("Game Over, press R to Restart!");

    }
}
