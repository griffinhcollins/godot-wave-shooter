using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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
    List<Improvement> excludePool;

    TextureRect damageGlow;

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
        damageGlow = GetNode<TextureRect>("DamageEffect/RedGlow");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("pause")) // hit esc
        {
            CanvasLayer optionsMenu = GetNode<CanvasLayer>("OptionsMenu");
            optionsMenu.Hide();
        }
        if (Input.IsActionJustPressed("restart") && (State.currentState == State.dead))
        {
            OnStartButtonPressed();
        }

        if (damageGlow.Visible)
        {
            Color mod = damageGlow.Modulate;
            damageGlow.Modulate = mod.Lerp(Colors.Transparent, 1 * (float)delta);
            if (damageGlow.Modulate.A < 0.1f)
            {
                damageGlow.Hide();
            }
        }
    }

    public void ShowDamageEffect()
    {
        damageGlow.Show();
        damageGlow.Modulate = Colors.White;
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
        shopElements.GetNode<Button>("Reroll").Hide();
        shopElements.GetNode<Button>("BuySlot").Hide();
    }

    private void OnNextWavePressed()
    {
        rerollCost = Stats.Counters.IsUnlockWave() ? 50 : 10;
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
        int cost = GetCurrentUpgradeSlotCost();
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
        if (player.ChargeMoney(50))
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

        Button buySlot = shopElements.GetNode<Button>("BuySlot");

        // Hide the upgrade slot purchase if the player is at max or it's an unlock wave
        if (upgradeSlotNum >= Stats.PlayerStats.UpgradeSlots.GetRange().Y || Stats.Counters.IsUnlockWave())
        {
            buySlot.Hide();
            buySlot.Disabled = true;
        }
        else
        {

            buySlot.Show();
            buySlot.Disabled = false;
        }
        upgradePool = Upgrades.GetAvailableUpgrades();
        excludePool = new();
        if (!delving)
        {
            for (int i = 0; i < (Stats.Counters.IsUnlockWave() ? 3 : upgradeSlotNum); i++)
            {
                UpgradeNode newUpgrade = AddUpgrade();
            }
            shopElements.GetNode<Button>("Reroll").Show();
        }
        else
        {
            // they paid to get an extra upgrade, but it'll come with a mutation
            for (int i = 0; i < 3; i++)
            {
                UpgradeNode newUpgrade = AddUpgrade(true);
            }
            // No rerolling or getting extra slots if you're delving
            buySlot.Hide();
            shopElements.GetNode<Button>("Reroll").Hide();
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
        rerollCost *= 2;
        foreach (UpgradeNode upgrade in shopElements.GetNode("Upgrades").GetChildren())
        {
            upgrade.QueueFree();
        }
        GenerateShop();
    }


    private void UpdateCosts()
    {
        shopElements.GetNode<Button>("BuySlot").GetNode<Label>("Cost").Text = string.Format("${0}", GetCurrentUpgradeSlotCost());
        shopElements.GetNode("Reroll").GetNode<Label>("Cost").Text = string.Format("${0}", rerollCost);
    }

    int GetCurrentUpgradeSlotCost()
    {
        return (int)(10 * Mathf.Pow(2, Stats.PlayerStats.UpgradeSlots.GetDynamicVal() - 3));
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

        GetNode<CanvasLayer>("Difficulty Menu").Show();
    }

    private void ChooseDifficulty(int difficulty)
    {
        // 0 is easy, 1 is medium, 2 is hard
        switch (difficulty)
        {
            case 0:
                State.difficulty = State.EASY;
                break;
            case 1:
                State.difficulty = State.MEDIUM;
                break;
            case 2:
                State.difficulty = State.HARD;
                break;
            default:
                GD.Print("fucked it");
                break;
        }
        ShowWave();
        GetNode<CanvasLayer>("MainMenuElements").Hide();
        GetNode<CanvasLayer>("Difficulty Menu").Hide();
        EmitSignal(SignalName.StartGame);
        gameOverElements.Hide();
    }

    private void OnOptionsButtonPressed()
    {

        GetNode<CanvasLayer>("OptionsMenu").Show();
        GetNode<ColorRect>("OptionsMenu/Credits").Hide();
    }

    private void OnOptionsBackPressed()
    {

        GetNode<CanvasLayer>("OptionsMenu").Hide();
    }

    private void OnCreditsBackPressed()
    {

        GetNode<ColorRect>("OptionsMenu/Credits").Hide();
    }

    private void OnSfxSliderAdjusted(float val)
    {
        int index = AudioServer.GetBusIndex("Sound Effects");
        GD.Print(AudioServer.GetBusVolumeDb(index));
        AudioServer.SetBusVolumeDb(index, 2 * Mathf.LinearToDb(val / 50));
    }

    private void OnMscSliderAdjusted(float val)
    {
        int index = AudioServer.GetBusIndex("Music");
        AudioServer.SetBusVolumeDb(index, 2 * Mathf.LinearToDb(val / 50));
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();

    }

    private void OnCreditsButtonPressed()
    {

        GetNode<ColorRect>("OptionsMenu/Credits").Show();
    }

    private void ExampleSound()
    {
        GD.Print("be");
        AudioStreamPlayer p = GetNode<AudioStreamPlayer>("OptionsMenu/ColorRect/Button/FireSound");
        p.Play();
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
        statsHolder.GetNode<Label>("WaveCount").Text = string.Format("Highest Wave Reached: {0}", Stats.Counters.WaveCounter.Value);
        statsHolder.GetNode<Label>("KillCount").Text = string.Format("Enemies Killed: {0}", Stats.Counters.KillCounter.Value);
        statsHolder.GetNode<Label>("CoinCount").Text = string.Format("Total Coins Collected: {0}", Stats.Counters.CoinCounter.Value);

    }
}
