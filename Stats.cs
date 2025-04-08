using Godot;
using System;
using System.Runtime.CompilerServices;

public static class Stats
{




    // Player attributes
    public static class PlayerStats
    {

        public static PlayerStat Damage = new PlayerStat(0, "Damage", 10);
        public static PlayerStat FireRate = new PlayerStat(1, "Firerate", 2);
        public static PlayerStat MaxHP = new PlayerStat(2, "HP", 3, true);
        public static PlayerStat HPReward = new PlayerStat(3, "HP Interest", 2, true);
        public static PlayerStat Multishot = new PlayerStat(4, "Multishot", 1);
        public static PlayerStat Spread = new PlayerStat(5, "Spread", 5, false, true);
        public static PlayerStat ShotSpeed = new PlayerStat(6, "Shot Speed", 1);
        public static PlayerStat DropRate = new PlayerStat(7, "Drop Rate", 1.5f);
        public static PlayerStat Bounces = new PlayerStat(8, "Bounces", 1, true);
        public static PlayerStat Pierces = new PlayerStat(9, "Pierces", 1, true);
        public static PlayerStat Speed = new PlayerStat(10, "Speed", 400, false, false, 0.5f);

        public static PlayerStat[] allStats = { Damage, FireRate, MaxHP, HPReward, Multishot, Spread, ShotSpeed, DropRate, Bounces, Pierces, Speed };


        public static void SetDefaults()
        {
            for (int i = 0; i < allStats.Length; i++)
            {
                allStats[i].Reset();
            }
            for (int i = 0; i < Unlocks.DynamicUnlocks.Length; i++)
            {
                Unlocks.DynamicUnlocks[i] = false;
            }
        }

        public static class Unlocks
        {

            public static class UnlockID
            {
                public static readonly int Laser = 0;
                public static readonly int Lich = 1;

                public static readonly string[] nameLookup = {
                "Laser Beam",
                "Lich"
            };




            }

            public static void ResetUnlockStats(int ID)
            {
                if (ID == UnlockID.Laser)
                {
                    for (int i = 0; i < LaserStats.BaseStats.Length; i++)
                    {
                        LaserStats.DynamicStats[i] = LaserStats.BaseStats[i];
                    }
                }
            }


            public static bool[] DynamicUnlocks = new bool[UnlockID.nameLookup.Length];



            public static class LaserStats
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





    public static void ResetStats()
    {
        PlayerStats.SetDefaults();
        EnemyStats.SetDefaults();
    }

    public static void PrintStats()
    {
        for (int i = 0; i < PlayerStats.allStats.Length; i++)
        {
            GD.Print(string.Format("{0} = {1}", PlayerStats.allStats[i].name, PlayerStats.allStats[i].GetDynamicVal()));
        }
    }


}
