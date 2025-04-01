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
            public static readonly int Bounces = 8;
            public static readonly int Pierces = 9;
            public static readonly int Speed = 10;



            public static readonly string[] nameLookup = {
                "Damage",
                "Firerate",
                "HP",
                "HP Interest",
                "Multishot",
                "Spread",
                "Shot Speed",
                "Drop Rate",
                "Bounces",
                "Pierces",
                "Speed"
            };


        }



        // Starting/default stats (what is used to calculate upgrades)

        public static readonly float[] BaseStats = {
            // Player Starting Stats
            10,      // 0: Damage
            2,      // 1: Firing Rate (Shots/second)
            3,      // 2: HP 
            2,      // 3: HPReward (How much extra money the player gets per remaining HP at the end of a wave)
            1,      // 4: Multishot (How many shots per fire. 1.5 means every shot is 50% of firing 1 and 50% of firing 2)
            5,      // 5: Spread (Angle in radians that shots can deviate)
            1,      // 6: Shot Speed (1000pixel/s that shots travel at)
            1.5f,   // 7: Drop Rate (Number of coins dropped by enemies killed)
            1,      // 8: Bouncing (After bouncing off this many mobs the bullet will self-destruct)
            1,      // 9: Piercing (After piercing through this many mobs the bullet will self-destruct) (MUTUALLY EXCLUSIVE WITH BOUNCING)
            1       // 10: Player speed (400p/s * sqrt(mult))
        };

        // Current stats
        public static float[] DynamicStats = new float[BaseStats.Length];

        public static void SetDefaults()
        {
            for (int i = 0; i < BaseStats.Length; i++)
            {
                DynamicStats[i] = BaseStats[i];
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
        for (int i = 0; i < PlayerStats.ID.nameLookup.Length; i++)
        {
            GD.Print(string.Format("{0} = {1}", PlayerStats.ID.nameLookup[i], PlayerStats.DynamicStats[i]));
        }
    }


}
