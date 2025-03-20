using Godot;
using System;

public partial class Hud : CanvasLayer
{
    [Signal]
    public delegate void StartGameEventHandler();

    public void UpdateWaveTime(int wavetime)
    {
        GetNode<Label>("WaveLabel").Text = wavetime.ToString();
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


    public void ShowGameOver()
    {
        ShowMessage("Game Over");

    }
}
