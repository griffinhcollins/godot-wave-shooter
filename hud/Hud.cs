using Godot;
using System;
using System.Collections.Generic;

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

    Label upgradeCost;
    HBoxContainer upgradeBar;

    List<PlayerUpgrade> upgradePool;

    Player player;
    public override void _Ready()
    {
        player = (Player)GetTree().GetNodesInGroup("player")[0];
        waveElements = GetNode<CanvasLayer>("WaveElements");
        shopElements = GetNode<CanvasLayer>("ShopElements");
        upgradeCost = shopElements.GetNode("BuySlot").GetNode<Label>("Cost");
    }


    public void ShowWave()
    {
        waveElements = GetNode<CanvasLayer>("WaveElements");
        shopElements = GetNode<CanvasLayer>("ShopElements");
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

    private void OnBuySlotClicked()
    {
        int currentUpgradeSlots = (int)Stats.PlayerStats.UpgradeSlots.GetDynamicVal();
        int cost = (currentUpgradeSlots - 1) * 5;
        if (player.ChargeMoney(cost))
        {
            AddUpgrade();
            Stats.PlayerStats.UpgradeSlots.ApplyUpgrade(1, true);
            if (Stats.PlayerStats.UpgradeSlots.GetDynamicVal() >= Stats.PlayerStats.UpgradeSlots.range.Y)
            {
                shopElements.GetNode<Button>("BuySlot").Hide();
                shopElements.GetNode<Button>("BuySlot").Disabled = true;
            }

            UpdateUpgradeSlotCost();
        }

    }

    public void UpdateWaveTime(int wavetime)
    {
        GetNode<Label>("WaveElements/WaveLabel").Text = wavetime.ToString();
    }

    public void ShowPauseMenu()
    {
        GetNode<CanvasLayer>("PauseElements").Show();
    }

    public void HidePauseMenu()
    {
        GetNode<CanvasLayer>("PauseElements").Hide();
    }

    private void OnResumeClicked()
    {
        GetParent<Main>().UnPause();
    }

    public void CreateDamageNumber(Vector2 pos, float amount)
    {
        Label dmgNum = damageNumber.Instantiate<Label>();
        dmgNum.Text = string.Format("{0:n0}", amount);
        dmgNum.Position = pos + new Vector2(GD.Randf() * 10 - 5, GD.Randf() * 10 - 5);
        AddChild(dmgNum);
    }

    public void GenerateShop()
    {
        upgradeBar = shopElements.GetNode<HBoxContainer>("Upgrades");
        ShowShop();
        int upgradeSlotNum = (int)Stats.PlayerStats.UpgradeSlots.GetDynamicVal();
        if (upgradeSlotNum >= Stats.PlayerStats.UpgradeSlots.range.Y)
        {
            shopElements.GetNode<Button>("BuySlot").Hide();
            shopElements.GetNode<Button>("BuySlot").Disabled = true;
        }
        else
        {

            shopElements.GetNode<Button>("BuySlot").Show();
            shopElements.GetNode<Button>("BuySlot").Disabled = false;
        }
        upgradePool = Upgrades.GetAllUpgrades();
        for (int i = 0; i < upgradeSlotNum; i++)
        {
            AddUpgrade();
        }
        // This is hacky but it will stop the upgrade bar sometimes appearing offset 
        upgradeBar.Visible = false;
        upgradeBar.Visible = true;
        UpdateUpgradeSlotCost();

    }

    private void UpdateUpgradeSlotCost()
    {
        shopElements.GetNode<Button>("BuySlot").GetNode<Label>("Cost").Text = string.Format("${0}", ((int)Stats.PlayerStats.UpgradeSlots.GetDynamicVal() - 1) * 5);
    }

    private void AddUpgrade()
    {
        UpgradeNode newUpgrade = upgrade.Instantiate<UpgradeNode>();
        upgradeBar.AddChild(newUpgrade);
        upgradePool = newUpgrade.Generate(upgradePool);
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
