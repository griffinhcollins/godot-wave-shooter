using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
    HFlowContainer upgradeContainer;

    List<PlayerUpgrade> upgradePool;
    List<Improvement> excludePool;

    TextureRect damageGlow;

    bool skippedForBonus = false;
    int bonusTaken = 0;


    Button nextWaveButton;

    int rerollCost = 10;
    bool upgradeSelected;

    Player player;
    public override void _Ready()
    {
        State.hud = this;
        player = (Player)GetTree().GetNodesInGroup("player")[0];
        waveElements = GetNode<CanvasLayer>("WaveElements");
        shopElements = GetNode<CanvasLayer>("ShopElements");
        gameOverElements = GetNode<CanvasLayer>("GameOverElements");
        upgradeCost = shopElements.GetNode("BuySlot").GetNode<Label>("Cost");
        damageGlow = GetNode<TextureRect>("DamageEffect/RedGlow");
        nextWaveButton = GetNode<Button>("ShopElements/NextWave");
        upgradeContainer = GetNode<HFlowContainer>("ShopElements/Upgrades/UpgradeContainer");
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
        skippedForBonus = false;
    }

    public void ShowShop()
    {
        player.Hide();
        waveElements.Hide();
        shopElements.Show();
        ResetUpgradeMenu();
        if (Stats.Counters.IsUnlockWave())
        {
            nextWaveButton.Text = "Skip Unlock (+3 free upgrades)";
            nextWaveButton.AddThemeFontSizeOverride("font_size", 36);

        }
        else
        {
            nextWaveButton.Text = "Skip Upgrade (+$10 money cap)";
            nextWaveButton.AddThemeFontSizeOverride("font_size", 36);

        }

        rerollCost = Stats.Counters.IsUnlockWave() ? 50 : 10;
        shopElements.GetNode<Button>("Delve Deeper").Hide();

    }

    private void ResetUpgradeMenu()
    {
        Control upgradesMenu = shopElements.GetNode<Control>("Upgrades");
        upgradesMenu.Show();
        ShowShopInfo();
    }

    public void ShowShopInfo()
    {
        UpdateUpgradeDescription("[font_size=50][center]Upgrade Store[/center][/font_size]\n\nDespite the name, upgrades are free! Rerolls and extra slots are not, however. Hover over an upgrade to see a description and the effect it will have on your stats. Click an upgrade to take it.\n\nIf none of the upgrades appeal to you, or you'd rather save up, you can skip the upgrades to increase your money cap by $10.");
    }

    private void OnUpgradeChosen()
    {
        // An upgrade has been selected
        if (!skippedForBonus || bonusTaken >= 2)
        {
            upgradeSelected = true;
            upgradeContainer.Hide();
            shopElements.GetNode<Control>("Upgrades").Hide();

            if (Stats.Counters.IsUnlockWave() && !skippedForBonus)
            {
                shopElements.GetNode<Button>("Delve Deeper").Show();
            }
            shopElements.GetNode<Button>("Reroll").Hide();
            shopElements.GetNode<Button>("BuySlot").Hide();

            // Change Skip for cash to Next Wave
            nextWaveButton.Text = "Next Wave";
            nextWaveButton.AddThemeFontSizeOverride("font_size", 64);
        }
        else
        {
            bonusTaken++;
        }

    }

    private void OnNextWavePressed()
    {
        if (upgradeSelected)
        {
            CloseShop();
            return;
        }
        if (Stats.Counters.IsUnlockWave())
        {
            // Player has skipped their unlock and is instead getting 3 free upgrades
            skippedForBonus = true;
            ShowBonusUpgrades();
        }
        else
        {
            // Player has skipped an upgrade so gets some money
            Stats.PlayerStats.MoneyCap.ApplyUpgrade(10, true);
            UpdateMoneyCounter(Stats.PlayerStats.Money); // Update the money counter
            // player.AddMoney(10);
            CloseShop();
        }
    }

    private void ShowBonusUpgrades()
    {
        // hoo boy
        // No rerolling or getting extra slots, they'll already have plenty of options
        bonusTaken = 0;
        upgradeSelected = true;
        nextWaveButton.Text = "Next Wave";
        UpdateUpgradeDescription("[font_size=50][center]Select 3 Upgrades![/center][/font_size]");
        nextWaveButton.AddThemeFontSizeOverride("font_size", 64);
        shopElements.GetNode<Button>("Reroll").Hide();
        shopElements.GetNode<Button>("BuySlot").Hide();
        foreach (UpgradeNode unlock in upgradeContainer.GetChildren())
        {
            unlock.QueueFree();
        }
        GenerateShop(10);

    }

    public void UpdateUpgradeDescription(string description)
    {

        shopElements.GetNode<RichTextLabel>("Upgrades/Description").Text = description;
    }

    private void CloseShop()
    {
        ShowWave();
        EmitSignal(SignalName.NextWave);
        foreach (UpgradeNode upgrade in upgradeContainer.GetChildren())
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

    public async Task PlayHPInterestAnimation()
    {
        foreach (TextureRect heart in GetNode<HBoxContainer>("WaveElements/HealthBar").GetChildren())
        {
            MoveHeart(heart);
            await ToSignal(GetTree().CreateTimer(0.4f), SceneTreeTimer.SignalName.Timeout);
            player.AddMoney((int)Stats.PlayerStats.HPReward.GetDynamicVal());
        }
    }
    async private void MoveHeart(TextureRect heart)
    {
        Vector2 targetPos = GetNode<Label>("MoneyLabel").GlobalPosition;
        Vector2 startPos = heart.GlobalPosition;
        for (int i = 0; i < 20; i++)
        {
            heart.GlobalPosition = startPos.Lerp(targetPos, i / 20f);
            await ToSignal(GetTree().CreateTimer(1 / 60f), SceneTreeTimer.SignalName.Timeout);
        }
        Label plusOne = damageNumber.Instantiate<Label>();
        plusOne.Text = string.Format("+{0:n0}", Stats.PlayerStats.HPReward.GetDynamicVal());
        plusOne.AddThemeColorOverride("font_color", Colors.Gold);
        plusOne.Position = heart.GlobalPosition + new Vector2(GD.Randf() * 10 - 5, GD.Randf() * 10 - 5);
        AddChild(plusOne);
        heart.QueueFree();
    }

    private void OnDelveDeeperPressed()
    {
        if (player.ChargeMoney(50))
        {
            foreach (var item in upgradeContainer.GetChildren())
            {
                item.QueueFree();
            }
            GenerateShop(3, true);

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

    public void GenerateShop(int upgradeNumOverride = -1, bool delving = false)
    {
        ShowShop();
        upgradeSelected = false;
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
            for (int i = 0; i < Mathf.Max(upgradeNumOverride, Stats.Counters.IsUnlockWave() ? 3 : upgradeSlotNum); i++)
            {
                UpgradeNode newUpgrade = AddUpgrade(delving, upgradeNumOverride != -1);
            }
            if (upgradeNumOverride == -1)
            {
                // Only allow rerolls if they're not getting bonus upgrades
                shopElements.GetNode<Button>("Reroll").Show();
            }
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
        upgradeContainer.Visible = false;
        upgradeContainer.Visible = true;
        UpdateCosts();

    }

    private void RerollShop()
    {
        if (upgradeSelected || !player.ChargeMoney(rerollCost))
        {
            return;
        }
        rerollCost *= 2;
        foreach (UpgradeNode upgrade in upgradeContainer.GetChildren())
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

    private UpgradeNode AddUpgrade(bool delving = false, bool upgradeOverride = false)
    {
        UpgradeNode newUpgrade = upgrade.Instantiate<UpgradeNode>();
        (upgradePool, excludePool) = newUpgrade.Generate(upgradePool, excludePool, delving, upgradeOverride);
        newUpgrade.UpgradeSelected += OnUpgradeChosen;
        upgradeContainer.AddChild(newUpgrade);
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
        GetNode<Label>("MoneyLabel").Text = string.Format("{0}/{1}", money.ToString(), Stats.PlayerStats.MoneyCap.GetDynamicVal());
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
