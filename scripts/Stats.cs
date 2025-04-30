using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;

public static class Stats
{




    // Player attributes
    public static class PlayerStats
    {

        public static PlayerStat Damage = new PlayerStat(0, "Damage", 10, new Vector2(5, Mathf.Inf));
        public static PlayerStat FireRate = new PlayerStat(1, "Firerate", 2, new Vector2(0.5f, 10));
        public static PlayerStat MaxHP = new PlayerStat(2, "HP", 3, new Vector2(0, 6), true);
        public static PlayerStat HPReward = new PlayerStat(3, "HP Interest", 2, new Vector2(0, 4), true);
        public static PlayerStat Multishot = new PlayerStat(4, "Multishot", 1, new Vector2(1, 10));
        public static PlayerStat Spread = new PlayerStat(5, "Spread", 5, new Vector2(0, 180), false, true);
        public static PlayerStat ShotSpeed = new PlayerStat(6, "Shot Speed", 1, new Vector2(0.25f, 10));
        public static PlayerStat DropRate = new PlayerStat(7, "Drop Rate", 1.5f, new Vector2(0.25f, 4));
        public static PlayerStat Bounces = new PlayerStat(8, "Bounces", 0, new Vector2(0, 10), true);
        public static PlayerStat Piercing = new PlayerStat(9, "Piercing Time", 0, new Vector2(0, 10), true);
        public static PlayerStat Speed = new PlayerStat(10, "Speed", 400, new Vector2(100, 1000), false, false, 0.5f);
        public static PlayerStat BulletSize = new PlayerStat(11, "Bullet Size", 1, new Vector2(0.25f, 3), false, false, 1);

        // Stats that have the generic "<stat> up" and "<stat> down" purchasable upgrades
        public static PlayerStat[] upgradeableStats = { Damage, FireRate, MaxHP, HPReward, Multishot, Spread, ShotSpeed, DropRate, Bounces, Piercing, Speed, BulletSize };


        public static void SetDefaults()
        {
            for (int i = 0; i < upgradeableStats.Length; i++)
            {
                upgradeableStats[i].Reset();
            }
            foreach (Unlockable u in Unlocks.allUnlockables)
            {
                u.unlocked = false;
                Unlocks.ResetUnlockStats(u);
            }
        }

        public static class Unlocks
        {

            // If you reach high enough shotspeed and firerate, you get LASER BEAM as an option
            public static Condition laser = new ConjunctCondition(new List<Condition> { new StatCondition(FireRate, 1.5f, true), new StatCondition(ShotSpeed, 1.5f, true), new StatCondition(Piercing, 2, true) });


            // Upgrades that can only show up if you reduce your max HP to less than 1 (ie 0)
            public static StatCondition lich = new StatCondition(MaxHP, 1, false);

            public static Unlockable Laser = new Unlockable("Laser Beam", laser);
            public static Unlockable WallBounce = new Unlockable("Wall Bounce", new StatCondition(Bounces, 2, true));


            public static Unlockable[] allUnlockables = { Laser, WallBounce };

            public static void ResetUnlockStats(Unlockable unlockable)
            {
                if (unlockable == Laser)
                {
                    for (int i = 0; i < LaserStats.BaseStats.Length; i++)
                    {
                        LaserStats.DynamicStats[i] = LaserStats.BaseStats[i];
                    }
                }
            }




            public static class LaserStats // TODO: actually implement this
            {
                public static readonly int lifetime = 0;

                public static readonly string[] nameLookup = {
                    "Lifetime"
                };

                public static readonly float[] BaseStats = {
                    // Laser Starting Stats
                    0.2f      // 0: lifetime (s)
                };

                // This gets reset when the laser pickup is first acquired
                public static readonly float[] DynamicStats = new float[BaseStats.Length];


                public static void ResetStats()
                {
                    for (int i = 0; i < BaseStats.Length; i++)
                    {
                        DynamicStats[i] = BaseStats[i];
                    }
                }

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
            public static readonly int SizeMult = 4;

            public static readonly string[] nameLookup = {
                "Wave Length",
                "Spawn Rate",
                "HP",
                "Acceleration",
                "Size"
            };


        }

        // Starting/Default Stats
        public static readonly float[] BaseStats = {
            // Enemy Starting Stats
            15,     // 0: Wave Length
            1,      // 1: Spawn Rate (Spawns/second)
            1,      // 2: HP Mult
            1,      // 3: Acceleration Mult 
            1       // 4: Size Mult
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
        public static void IncreaseDifficulty()
        {

            DynamicStats[ID.SpawnRate] *= 1.2f;

            int mutationIndex = GD.RandRange(0, DynamicStats.Length - 1); // only have as many as you have mutation upgrades
            LastMutation = string.Format("{0} increased!", ID.nameLookup[mutationIndex]);
            DynamicStats[mutationIndex] *= 1.5f;
        }

    }





    public static void ResetStats()
    {
        PlayerStats.SetDefaults();
        EnemyStats.SetDefaults();
    }

    public static void PrintStats()
    {
        for (int i = 0; i < PlayerStats.upgradeableStats.Length; i++)
        {
            GD.Print(string.Format("{0} = {1}", PlayerStats.upgradeableStats[i].name, PlayerStats.upgradeableStats[i].GetDynamicVal()));
        }
    }


}
