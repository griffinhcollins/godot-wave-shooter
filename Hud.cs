using Godot;
using System;

public partial class Hud : CanvasLayer
{
    [Signal]
    public delegate void StartGameEventHandler();

    [Signal]
    public delegate void NextWaveEventHandler();

    [Export]
    public PackedScene heart;
    [Export]
    public PackedScene upgrade;
    [Export]
    public PackedScene damageNumber;

    CanvasLayer waveElements;
    CanvasLayer shopElements;


    Player player;
    public override void _Ready()
    {

        player = (Player)GetTree().GetNodesInGroup("player")[0];
        waveElements = GetNode<CanvasLayer>("WaveElements");
        shopElements = GetNode<CanvasLayer>("ShopElements");

    }


    public void ShowWave()
    {
        waveElements.Show();
        shopElements.Hide();
    }

    public void ShowShop()
    {
        player.Hide();
        waveElements.Hide();
        shopElements.Show();
    }

    private void OnNextWavePressed()
    {
        ShowWave();
        EmitSignal(SignalName.NextWave);
        foreach (UpgradeNode upgrade in shopElements.GetNode("Upgrades").GetChildren())
        {
            upgrade.QueueFree();
        }
    }

    public void UpdateWaveTime(int wavetime)
    {
        GetNode<Label>("WaveElements/WaveLabel").Text = wavetime.ToString();
    }

    public void CreateDamageNumber(Vector2 pos, float amount){
        Label dmgNum = damageNumber.Instantiate<Label>();
        dmgNum.Text = string.Format("{0:n0}", amount);
        dmgNum.Position = pos;
        AddChild(dmgNum);
    }

    public void GenerateShop()
    {
        ShowShop();
        HBoxContainer upgradeBar = shopElements.GetNode<HBoxContainer>("Upgrades");
        for (int i = 0; i < 3; i++)
        {
            upgradeBar.AddChild(upgrade.Instantiate<UpgradeNode>());
        }
        // This is hacky but it will stop the upgrade bar sometimes appearing offset 
        upgradeBar.Visible = false;
        upgradeBar.Visible = true;
    }

    public void UpdateHealth(int HP)
    {
        HBoxContainer healthbar = GetNode<HBoxContainer>("WaveElements/HealthBar");
        foreach (TextureRect heart in healthbar.GetChildren())
        {
            heart.QueueFree();
        }
        for (int i = 0; i < HP; i++)
        {
            healthbar.AddChild(heart.Instantiate<TextureRect>());
        }
    }

    public void HideMessage()
    {
        GetNode<Label>("WaveElements/Message").Hide();
        GetNode<Label>("ShopElements/Message").Hide();
    }

    public void ShowMessage(string text, bool timeout = true)
    {
        Label message;
        if (waveElements.Visible)
        {
            message = GetNode<Label>("WaveElements/Message");

        }
        else
        {
            message = GetNode<Label>("ShopElements/Message");
        }
        message.Text = text;
        message.Show();

        if (timeout)
        {
            GetNode<Timer>("MessageTimer").Start();

        }
    }

    private void OnStartButtonPressed()
    {
        ShowWave();
        GetNode<Button>("WaveElements/StartButton").Hide();
        EmitSignal(SignalName.StartGame);
    }


    private void OnMessageTimerTimeout()
    {
        GetNode<Label>("WaveElements/Message").Hide();
        GetNode<Label>("ShopElements/Message").Hide();
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
