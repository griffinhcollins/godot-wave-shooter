using Godot;
using static Upgrades;
using System;
using System.Linq;
using System.Collections.Generic;
using static RarityControl;
using System.Diagnostics.CodeAnalysis;


public partial class UpgradeNode : Button
{

    Hud hud;

    HFlowContainer iconHolder;

    PlayerUpgrade upgrade;
    float magnitude;
    Rarity rarity;

    List<Prerequisite> prereqs;
    bool delving = false;

    bool locked = false; // Sometimes we show locked upgrades if there aren't any unlocked ones to show

    [Signal]
    public delegate void UpgradeSelectedEventHandler();


    int cost;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {


    }

    public (List<PlayerUpgrade>, List<Prerequisite>) Generate(List<PlayerUpgrade> upgradePool, List<Prerequisite> exclude, bool _delving)
    {
        if (exclude is null)
        {
            exclude = new List<Prerequisite>();
        }
        delving = _delving;
        hud = (Hud)GetTree().GetNodesInGroup("HUD")[0];
        iconHolder = GetNode<HFlowContainer>("IconHolder");
        (upgradePool, exclude) = Randomize(upgradePool, exclude);

        UpdateCost();
        return (upgradePool, exclude);
    }

    // The pool is already checked for conditions, and any upgrades that have previously appeared in this shop are added to exclude
    (List<PlayerUpgrade>, List<Prerequisite>) Randomize(List<PlayerUpgrade> pool, List<Prerequisite> exclude)
    {
        if (Stats.Counters.IsUnlockWave())
        {
            // it's fuckin unlock time baby

            List<PlayerUnlockUpgrade> allUnlockUpgrades = pool.OfType<PlayerUnlockUpgrade>().Where(u => !exclude.Contains(u.unlock)).ToList();
            if (allUnlockUpgrades.Count >= 1) // Do we have unlockables to give?
            {
                upgrade = allUnlockUpgrades[GD.RandRange(0, allUnlockUpgrades.Count - 1)];
                magnitude = 1;
                AddIcon(upgrade);
                SelfModulate = Legendary.colour;
                exclude.Add(((PlayerUnlockUpgrade)upgrade).unlock);
            }
            else
            {
                // show an unlockable that's locked behind a prerequisite we haven't met, but tell the player the prereq
                foreach (Unlockable u in Stats.PlayerStats.Unlocks.allUnlockables.Where(u => !exclude.Contains(u)).OrderBy(x => GD.Randf()))
                {
                    // It's not unlocked and it's not available to unlock
                    if (u.unlocked == false && !u.CheckCondition())
                    {
                        prereqs = new List<Prerequisite>();
                        foreach (Prerequisite p in u.GetPrerequisites())
                        {
                            prereqs.Add(p);
                        }
                        upgrade = new PlayerUnlockUpgrade(u);
                        locked = true;
                        AddIcon(upgrade);
                        SelfModulate = new Color(0.1f, 0.1f, 0.1f);
                        exclude.Add(((PlayerUnlockUpgrade)upgrade).unlock);
                        return (pool, exclude);
                    }
                }
                if (!locked)
                {
                    // we're completely out
                    Hide();
                    return (pool, exclude);
                }

            }
        }
        else
        {
            // This wave, only stat upgrades
            List<PlayerStatUpgrade> allStatUpgrades = pool.OfType<PlayerStatUpgrade>().Where(u => u.IsPositive()).Where(u => !exclude.Contains(u.stat)).ToList();
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
            exclude.Add(((PlayerStatUpgrade)upgrade).stat);
        }
        return (pool, exclude);
    }





    private void OnMouseOver()
    {
        string infoMessage = "";

        infoMessage += upgrade.GetDescription(magnitude);
        if (locked)
        {
            infoMessage += "\nLocked, requires more ";
            foreach (Prerequisite p in prereqs)
            {
                infoMessage += p.GetName();
                infoMessage += ", ";
            }
            infoMessage = infoMessage.Remove(infoMessage.Length - 2);
        }
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
        if (locked)
        {
            return;
        }
        upgrade.Execute(magnitude);
        if (delving)
        {
            List<Mutation> availableMuts = Stats.PlayerStats.Mutations.GetAvailableMutations();
            Stats.PlayerStats.Mutations.SetMutation(availableMuts[GD.RandRange(0, availableMuts.Count - 1)]);
        }
        EmitSignal(SignalName.UpgradeSelected);
        QueueFree();

    }

    // strength can be 0 (standard), 1 (extended) or 2+ (extreme)
    private float CalculateMagnitude(bool intChange, int strength, float changePolynomial)
    {
        if (intChange)
        {
            return Math.Max(strength, (int)changePolynomial);

        }
        else
        {
            float x = (0.24f + GD.Randf() / 4) * (1 + strength / 2);

            return Mathf.Pow(x, 1 / changePolynomial);
        }


    }

    void UpdateCost()
    {
        GetNode<Label>("Cost").Hide();
        // GetNode<Label>("Cost").Text = string.Format("${0}", cost);
    }

    private void AddIcon(PlayerUpgrade upgrade)
    {
        TextureRect textureRect = new TextureRect();
        textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
        textureRect.CustomMinimumSize = new Vector2(120, 120);
        textureRect.Texture = upgrade.GetUpgradeIcon();
        iconHolder.AddChild(textureRect);
        if (delving)
        {
            TextureRect mutationRect = new TextureRect();
            mutationRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            mutationRect.CustomMinimumSize = new Vector2(120, 120);
            mutationRect.Texture = (Texture2D)GD.Load("res://custom assets/hud/mutated upgrade.png");
            mutationRect.Rotation = (float)(GD.Randi() % 4) * Mathf.Pi / 2f;
            textureRect.AddChild(mutationRect);
        }
        if (locked)
        {
            TextureRect lockRect = new TextureRect();
            lockRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            lockRect.CustomMinimumSize = new Vector2(120, 120);
            lockRect.Texture = (Texture2D)GD.Load("res://custom assets/upgrade icons/lock.png");
            textureRect.AddChild(lockRect);
        }

    }

}
