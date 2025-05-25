using Godot;
using static Upgrades;
using System;
using System.Linq;
using System.Collections.Generic;
using static RarityControl;


public partial class UpgradeNode : Button
{

    Hud hud;

    HFlowContainer iconHolder;

    PlayerUpgrade upgrade;
    float magnitude;
    Rarity rarity;

    [Signal]
    public delegate void UpgradeSelectedEventHandler();


    int cost;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {


    }

    public List<PlayerUpgrade> Generate(List<PlayerUpgrade> upgradePool)
    {
        hud = (Hud)GetTree().GetNodesInGroup("HUD")[0];
        iconHolder = GetNode<HFlowContainer>("IconHolder");
        upgradePool = Randomize(upgradePool);
        UpdateCost();
        return upgradePool;
    }

    // The pool is already checked for conditions, and any upgrades that have previously appeared in this shop are removed
    List<PlayerUpgrade> Randomize(List<PlayerUpgrade> pool)
    {
        if (Stats.Counters.WaveCounter % 3 == 0)
        {
            // it's fuckin unlock time baby
            List<PlayerUnlockUpgrade> allUnlockUpgrades = pool.OfType<PlayerUnlockUpgrade>().ToList();
            upgrade = allUnlockUpgrades[GD.RandRange(0, allUnlockUpgrades.Count - 1)];
            magnitude = 1;
            AddIcon(upgrade);
            SelfModulate = Legendary.colour;
        }
        else
        {
            // This wave, only stat upgrades
            List<PlayerStatUpgrade> allStatUpgrades = pool.Where(u => u.IsPositive()).OfType<PlayerStatUpgrade>().ToList();
            // Roll to see what rarity we get
            float roll = GD.Randf();
            if (roll < 0.5f)
            {
                rarity = Common;
            }
            else
            {
                if (roll < 0.90f)
                {
                    rarity = Uncommon;
                }
                else
                {
                    rarity = Rare;
                }
            }
            SelfModulate = rarity.colour;
            List<PlayerStatUpgrade> rarityPool = allStatUpgrades.Where(u => u.stat.rarity == rarity).ToList();
            if (rarityPool.Count == 0)
            {
                rarityPool = allStatUpgrades;
            }
            upgrade = rarityPool[GD.RandRange(0, rarityPool.Count - 1)];
            AddIcon(upgrade);
            PlayerStat stat = ((PlayerStatUpgrade)upgrade).stat;
            magnitude = CalculateMagnitude(stat.intChange, 1, stat.changePolynomial);
        }
        pool.Remove(upgrade);
        return pool;
    }





    private void OnMouseOver()
    {
        string infoMessage = "";

        infoMessage += upgrade.GetDescription(magnitude);
        // infoMessage += string.Format(", ({0})", rarity.name);
        hud.ShowMessage(infoMessage, false);
    }

    private void OnMouseLeave()
    {
        hud.HideMessage();
    }

    private void OnClicked()
    {
        // if (!((Player)GetTree().GetNodesInGroup("player")[0]).ChargeMoney(cost)) // Can't afford it mate
        // {
        //     return;
        // }

        upgrade.Execute(magnitude);
        EmitSignal(SignalName.UpgradeSelected);
        QueueFree();

    }

    // strength can be 0 (standard), 1 (extended) or 2+ (extreme)
    private float CalculateMagnitude(bool intChange, int strength, float changePolynomial)
    {
        if (intChange)
        {
            return Math.Max(strength, 1);

        }
        else
        {
            float x = (0.24f + GD.Randf() / 4) * (1 + strength / 2);

            return Mathf.Pow(x, 1 / changePolynomial);
        }


    }

    void UpdateCost()
    {
        GetNode<Label>("Cost").Text = string.Format("${0}", cost);
    }

    private void AddIcon(PlayerUpgrade upgrade)
    {
        TextureRect textureRect = new TextureRect();
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        textureRect.CustomMinimumSize = new Vector2(45, 45);
        textureRect.Texture = upgrade.GetUpgradeIcon();
        iconHolder.AddChild(textureRect);
    }

}
