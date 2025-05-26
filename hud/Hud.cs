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
    CanvasLayer gameOverElements;

    Label upgradeCost;
    HBoxContainer upgradeBar;

    List<PlayerUpgrade> upgradePool;
    List<Prerequisite> excludePool;

    int rerollCost = 10;
    bool upgradeSelected;

    Player player;
    public override void _Ready()
    {
        player = (Player)GetTree().GetNodesInGroup("player")[0];
        waveElements = GetNode<CanvasLayer>("WaveElements");
        shopElements = GetNode<CanvasLayer>("ShopElements");
        gameOverElements = GetNode<CanvasLayer>("GameOverElements");
        upgradeCost = shopElements.GetNode("BuySlot").GetNode<Label>("Cost");
    }


    public void ShowWave()
    {
        waveElements = GetNode<CanvasLayer>("WaveElements");
        shopElements = GetNode<CanvasLayer>("ShopElements");
        waveElements.Show();
        shopElements.Hide();
        gameOverElements.Hide();
    }

    public void ShowShop()
    {
        player.Hide();
        waveElements.Hide();
        shopElements.Show();
        shopElements.GetNode<Button>("Delve Deeper").Hide();

    }

    private void OnUpgradeChosen()
    {
        // An upgrade has been selected
        upgradeSelected = true;
        upgradeBar.Hide();
        if (Stats.Counters.IsUnlockWave())
        {
            shopElements.GetNode<Button>("Delve Deeper").Show();
        }
    }

    private void OnNextWavePressed()
    {
        rerollCost = 10;
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
            if (Stats.PlayerStats.UpgradeSlots.GetDynamicVal() >= Stats.PlayerStats.UpgradeSlots.GetRange().Y)
            {
                shopElements.GetNode<Button>("BuySlot").Hide();
                shopElements.GetNode<Button>("BuySlot").Disabled = true;
            }

            UpdateCosts();
        }

    }

    private void OnDelveDeeperPressed()
    {
        if (player.ChargeMoney(30))
        {
            foreach (var item in upgradeBar.GetChildren())
            {
                item.QueueFree();
            }
            GenerateShop(true);

        }
    }

    public void ClearGameOver()
    {
        gameOverElements.Hide();
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

    public void GenerateShop(bool delving = false)
    {
        upgradeSelected = false;
        upgradeBar = shopElements.GetNode<HBoxContainer>("Upgrades");
        ShowShop();
        int upgradeSlotNum = (int)Stats.PlayerStats.UpgradeSlots.GetDynamicVal();
        // Hide the upgrade slot purchase if the player is at max
        if (upgradeSlotNum >= Stats.PlayerStats.UpgradeSlots.GetRange().Y)
        {
            shopElements.GetNode<Button>("BuySlot").Hide();
            shopElements.GetNode<Button>("BuySlot").Disabled = true;
        }
        else
        {

            shopElements.GetNode<Button>("BuySlot").Show();
            shopElements.GetNode<Button>("BuySlot").Disabled = false;
        }
        upgradePool = Upgrades.GetAvailableUpgrades();
        excludePool = new();
        if (!delving)
        {
            for (int i = 0; i < upgradeSlotNum; i++)
            {
                UpgradeNode newUpgrade = AddUpgrade();
            }
        }
        else
        {
            // they paid $20 to get an extra upgrade, but it'll come with a mutation
            for (int i = 0; i < 3; i++)
            {
                UpgradeNode newUpgrade = AddUpgrade(true);
            }
        }
        // This is hacky but it will stop the upgrade bar sometimes appearing offset 
        upgradeBar.Visible = false;
        upgradeBar.Visible = true;
        UpdateCosts();

    }

    private void RerollShop()
    {
        if (upgradeSelected || !player.ChargeMoney(rerollCost))
        {
            return;
        }
        rerollCost += 5;
        foreach (UpgradeNode upgrade in shopElements.GetNode("Upgrades").GetChildren())
        {
            upgrade.QueueFree();
        }
        GenerateShop();
    }


    private void UpdateCosts()
    {
        shopElements.GetNode<Button>("BuySlot").GetNode<Label>("Cost").Text = string.Format("${0}", ((int)Stats.PlayerStats.UpgradeSlots.GetDynamicVal() - 1) * 5);
        shopElements.GetNode("Reroll").GetNode<Label>("Cost").Text = string.Format("${0}", rerollCost);
    }

    private UpgradeNode AddUpgrade(bool delving = false)
    {
        UpgradeNode newUpgrade = upgrade.Instantiate<UpgradeNode>();
        upgradeBar.AddChild(newUpgrade);
        (upgradePool, excludePool) = newUpgrade.Generate(upgradePool, excludePool, delving);
        newUpgrade.UpgradeSelected += OnUpgradeChosen;
        return newUpgrade;
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
        gameOverElements.Hide();
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

        gameOverElements.Show();
        VBoxContainer statsHolder = gameOverElements.GetNode<Label>("Stats").GetNode<VBoxContainer>("VBoxContainer");
        statsHolder.GetNode<Label>("WaveCount").Text = string.Format("Highest Wave Reached: {0}", Stats.Counters.WaveCounter);
        statsHolder.GetNode<Label>("KillCount").Text = string.Format("Enemies Killed: {0}", Stats.Counters.KillCounter);
        statsHolder.GetNode<Label>("CoinCount").Text = string.Format("Total Coins Collected: {0}", Stats.Counters.CoinCounter);

    }
}
