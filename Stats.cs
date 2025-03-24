using Godot;
using System;
using System.Runtime.CompilerServices;

public static class Stats
{

    // Player attributes
    public static class PlayerStats
    {

        public static class ID
        {
            public static readonly int Damage = 0;
            public static readonly int FireRate = 1;
            public static readonly int HP = 2;
            public static readonly int HPReward = 3;
            public static readonly int Multishot = 4;
            public static readonly int Spread = 5;
            public static readonly int ShotSpeed = 6;
            public static readonly int DropRate = 7;

            public static readonly string[] nameLookup = {
                "Damage",
                "Firerate",
                "HP",
                "HP Interest",
                "Multishot",
                "Spread",
                "Shot Speed",
                "Drop Rate"
            };


        }


        // Starting/default stats (what is used to calculate upgrades)

        public static readonly float[] BaseStats = {
            // Player Starting Stats
            1,      // 0: Damage
            2,      // 1: Firing Rate (Shots/second)
            3,      // 2: HP 
            2,      // 3: HPReward (How much extra money the player gets per remaining HP at the end of a wave)
            1,      // 4: Multishot (How many shots per fire. 1.5 means every shot is 50% of firing 1 and 50% of firing 2)
            5,      // 5: Spread (Angle in radians that shots can deviate)
            1,      // 6: Shot Speed (1000pixel/s that shots travel at)
            1.5f    // 7: Drop Rate (Number of coins dropped by enemies killed)
        };

        // Current stats
        public static float[] DynamicStats = new float[BaseStats.Length];

        public static void SetDefaults()
        {
            for (int i = 0; i < BaseStats.Length; i++)
            {
                DynamicStats[i] = BaseStats[i];
            }
        }

    }


    // Enemy attributes
    public static class EnemyStats
    {

        public static class ID
        {
            public static readonly int WaveLength = 0;
            public static readonly int SpawnRate = 1;
            public static readonly int HPMult = 2;
            public static readonly int AccelerationMult = 3;
            public static readonly int DropRate = 4;

            public static readonly string[] nameLookup = {
                "Wave Length",
                "Spawn Rate",
                "HP",
                "Acceleration"
            };


        }

        // Starting/Default Stats
        public static readonly float[] BaseStats = {
            // Enemy Starting Stats
            15,     // 0: Wave Length
            1,      // 1: Spawn Rate (Spawns/second)
            1,      // 2: HP Mult
            1,      // 3: Acceleration Mult 
        };

        // Current stats
        public static float[] DynamicStats = new float[BaseStats.Length];

        public static void SetDefaults()
        {
            for (int i = 0; i < BaseStats.Length; i++)
            {
                DynamicStats[i] = BaseStats[i];
            }
        }

        public static string LastMutation;
        public static void IncreaseRandomStats()
        {
            int mutationIndex = GD.RandRange(0, DynamicStats.Length - 1); // only have as many as you have mutation upgrades
            LastMutation = string.Format("{0} increased!", ID.nameLookup[mutationIndex]);
            DynamicStats[mutationIndex] *= 1.5f;
        }

    }


    public static class Upgrades
    {

        public static PlayerUpgrade dmgUp = new PlayerUpgrade(0, PlayerStats.ID.Damage, "Damage Up", false, true, true, "dmg_up.png");
        public static PlayerUpgrade dmgDown = new PlayerUpgrade(1, PlayerStats.ID.Damage, "Damage Down", false, false, false, "dmg_down.png");
        public static PlayerUpgrade firerateUp = new PlayerUpgrade(2, PlayerStats.ID.FireRate, "Firerate Up", false, true, true, "firerate_up.png");
        public static PlayerUpgrade firerateDown = new PlayerUpgrade(3, PlayerStats.ID.FireRate, "Firerate Down", false, false, false, "firerate_down.png");
        public static PlayerUpgrade hpUp = new PlayerUpgrade(4, PlayerStats.ID.HP, "HP Up", true, true, true, "hp_up.png");
        public static PlayerUpgrade hpDown = new PlayerUpgrade(5, PlayerStats.ID.HP, "HP Down", true, false, false, "hp_down.png");
        public static PlayerUpgrade hpRewardUp = new PlayerUpgrade(6, PlayerStats.ID.HPReward, "HP Interest Up", true, true, true, "hpreward_up.png");
        public static PlayerUpgrade hpRewardDown = new PlayerUpgrade(7, PlayerStats.ID.HPReward, "HP Interest Down", true, false, false, "hpreward_down.png");
        public static PlayerUpgrade droprateUp = new PlayerUpgrade(8, PlayerStats.ID.DropRate, "Coin Droprate Up", false, true, true, "moneyrate_up.png");
        public static PlayerUpgrade droprateDown = new PlayerUpgrade(9, PlayerStats.ID.DropRate, "Coin Droprate Down", false, false, false, "moneyrate_down.png");
        public static PlayerUpgrade multishotUp = new PlayerUpgrade(10, PlayerStats.ID.Multishot, "Multishot Up", false, true, true, "multishot_up.png");
        public static PlayerUpgrade multishotDown = new PlayerUpgrade(11, PlayerStats.ID.Multishot, "Multishot Down", false, false, false, "multishot_down.png");
        public static PlayerUpgrade spreadUp = new PlayerUpgrade(12, PlayerStats.ID.Spread, "Spread Up", false, false, true, "spread_up.png");
        public static PlayerUpgrade spreadDown = new PlayerUpgrade(13, PlayerStats.ID.Spread, "Spread Down", false, true, false, "spread_down.png");

        public static PlayerUpgrade[] allUpgrades = { dmgUp, dmgDown, firerateUp, firerateDown, hpUp, hpDown, hpRewardUp, hpRewardDown, droprateUp, droprateDown, multishotUp, multishotDown, spreadUp, spreadDown };

    }


    public static void ResetStats()
    {
        PlayerStats.SetDefaults();
        EnemyStats.SetDefaults();
    }

    public static void PrintStats()
    {
        for (int i = 0; i < PlayerStats.ID.nameLookup.Length; i++)
        {
            GD.Print(string.Format("{0} = {1}", PlayerStats.ID.nameLookup[i], PlayerStats.DynamicStats[i]));
        }
    }


}
