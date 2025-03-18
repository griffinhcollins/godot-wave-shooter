using Godot;
using System;

public partial class Hud : CanvasLayer
{
    [Signal]
    public delegate void StartGameEventHandler();

    public void UpdateScore(int score)
    {
        GetNode<Label>("ScoreLabel").Text = score.ToString();
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


    async public void ShowGameOver()
    {
        Timer messageTimer = GetNode<Timer>("MessageTimer");
        ShowMessage("Game Over");
        await ToSignal(messageTimer, Timer.SignalName.Timeout);
        Label message = GetNode<Label>("Message");
        message.Text = "Dodge the Creeps!";
        message.Show();

        await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
        GetNode<Button>("StartButton").Show();

    }
}
