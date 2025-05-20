using Godot;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

public static class Stats
{


    public class StatSet
    {
        // Stats that, once this set is added to the pool/unlocked/whatever, can roll upgrades up and down in the shop
        public List<PlayerStat> upgradeableStats;
        // The condition for this statset to start appearing in the shop
        Condition condition;

        public StatSet(List<PlayerStat> _stats, Condition _condition = null)
        {
            upgradeableStats = _stats;
            AddCondition(_condition);
        }


        public void AddCondition(Condition newCondition)
        {
            if (newCondition is null)
            {
                return;
            }
            condition = newCondition.And(condition);
            foreach (PlayerStat stat in upgradeableStats)
            {
                stat.condition = condition.And(stat.condition);
            }
        }

        public void SetToDefaultStartingValues()
        {
            for (int i = 0; i < upgradeableStats.Count; i++)
            {
                upgradeableStats[i].Reset();
            }

        }

        public void SetToZero()
        {
            for (int i = 0; i < upgradeableStats.Count; i++)
            {
                upgradeableStats[i].Nullify();
            }

        }

        public List<PlayerStatUpgrade> GenerateUpgrades()
        {
            List<PlayerStatUpgrade> myUpgrades = new List<PlayerStatUpgrade>();
            foreach (PlayerStat stat in upgradeableStats)
            {
                myUpgrades.Add(stat.GenerateIncreasingUpgrade());
                myUpgrades.Add(stat.GenerateDecreasingUpgrade());
            }
            return myUpgrades;
        }

    }



    // Player attributes
    public static class PlayerStats
    {

        static List<PlayerUpgrade> allUpgrades;

        // This holds every stat set in the game
        static List<StatSet> allStatSets;
        // This holds every stat set in the game that is currently unlocked
        static List<StatSet> allAvailableStatSets;

        // Default starting stats

        public static PlayerStat Damage = new PlayerStat("Damage", 10, new Vector2(5, Mathf.Inf));
        public static PlayerStat FireRate = new PlayerStat("Firerate", 2, new Vector2(0.5f, 10));
        public static PlayerStat MaxHP = new PlayerStat("HP", 3, new Vector2(0, 6), true);
        public static PlayerStat HPReward = new PlayerStat("HP Interest", 1, new Vector2(0, 4), true);
        public static PlayerStat Multishot = new PlayerStat("Multishot", 1, new Vector2(1, 10));
        public static PlayerStat Spread = new PlayerStat("Spread", 5, new Vector2(0, 180), false, true);
        public static PlayerStat ShotSpeed = new PlayerStat("Shot Speed", 1, new Vector2(0.25f, 10));
        public static PlayerStat DropRate = new PlayerStat("Drop Rate", 1.5f, new Vector2(0.25f, 4));
        public static PlayerStat Speed = new PlayerStat("Speed", 400, new Vector2(100, 1000), false, false, 0.5f);
        public static PlayerStat BulletSize = new PlayerStat("Bullet Size", 1, new Vector2(0.25f, 3), false, false, 0.5f);
        public static PlayerStat Lifetime = new PlayerStat("Bullet Lifetime", 3, new Vector2(3, 20));
        static List<PlayerStat> defaultStatList = new List<PlayerStat> { Damage, FireRate, MaxHP, HPReward, Multishot, Spread, ShotSpeed, DropRate, Speed, BulletSize, Lifetime };

        static StatSet defaultStats = new StatSet(defaultStatList);

        public static List<PlayerStat> GetDefaultStats()
        {
            return defaultStatList;
        }

        // Misc. Stats (not put into statsets because they don't have a combined condition or upgrades in the shop)
        public static int Money;
        public static PlayerStat UpgradeSlots = new PlayerStat("Upgrade Slots", 3, new Vector2(3, 7), true);
        public static List<PlayerStat> miscellaneousStats = new List<PlayerStat> { UpgradeSlots };


        // Gets every possible upgrade that could be purchased from the store in the entire game
        public static List<PlayerUpgrade> GetAllUpgrades()
        {
            // Start with the defaults
            List<PlayerUpgrade> allAvailableUpgrades = defaultStats.GenerateUpgrades().ToList<PlayerUpgrade>();
            // Add every unlockable and all stat upgrades associated with that unlockable
            foreach (Unlockable u in Unlocks.allUnlockables)
            {
                allAvailableUpgrades.Add(new PlayerUnlockUpgrade(u));
                allAvailableUpgrades.AddRange(u.associatedStats.GenerateUpgrades());
            }
            allUpgrades = allAvailableUpgrades;
            return allAvailableUpgrades;
        }


        public static void SetDefaults()
        {
            Money = 1000;
            defaultStats.SetToDefaultStartingValues();
            for (int i = 0; i < miscellaneousStats.Count; i++)
            {
                miscellaneousStats[i].Reset();
            }
            foreach (Unlockable u in Unlocks.allUnlockables)
            {
                u.Reset();
            }
            foreach (Mutation m in Mutations.allMutations)
            {
                m.applied = false;
            }
        }

        public static class Unlocks
        {

            // Bouncing bullets
            public static PlayerStat bouncingBulletBounces = new PlayerStat("Bounces", 1, new Vector2(0, 10), true);
            static List<PlayerStat> bouncingBulletsStatList = new List<PlayerStat> { bouncingBulletBounces };
            static StatSet bouncingBulletsStats = new StatSet(bouncingBulletsStatList);
            public static Unlockable BouncingBullets = new Unlockable("Bouncing Bullets", bouncingBulletsStats);


            // Wallbounce, it's a direct upgrade to bouncing bullets
            static Condition wallBounceUnlockCondition = new ConjunctCondition(new List<Condition> { new StatCondition(bouncingBulletBounces, 2, true), new UnlockCondition(BouncingBullets, true) });
            public static PlayerStat wallBounceDamageRetention = new PlayerStat("Wall Bounce Damage Retention", 0.5f, new Vector2(0.5f, 1), false, false, 0.5f);
            static List<PlayerStat> wallBounceStatList = new List<PlayerStat> { wallBounceDamageRetention };
            static StatSet wallBounceStats = new StatSet(wallBounceStatList, wallBounceUnlockCondition);
            public static Unlockable WallBounce = new Unlockable("Wall Bounce", wallBounceStats, wallBounceUnlockCondition);

            // Piercing
            public static PlayerStat piercingBulletsPiercingTime = new PlayerStat("Piercing Time", 0.25f, new Vector2(0, 2), false, false, 0.5f);
            static List<PlayerStat> piercingBulletsStatList = new List<PlayerStat> { piercingBulletsPiercingTime };
            static StatSet piercingBulletsStats = new StatSet(piercingBulletsStatList);
            public static Unlockable PiercingBullets = new Unlockable("Piercing Bullets", piercingBulletsStats);

            // Piercing overwhammer style
            static Condition overflowCondition = new ConjunctCondition(new List<Condition> { new StatCondition(piercingBulletsPiercingTime, 0.5f, true), new StatCondition(Damage, 20, true) });

            // What fraction of the damage is reduced when hitting an enemy, e.g. if the enemy has 10 health and this stat is 0.5f, the overflow loses 5 damage
            public static PlayerStat overflowLoss = new PlayerStat("Overflow Reduction", 1, new Vector2(0, 1), false, true);
            static List<PlayerStat> overflowStatList = new List<PlayerStat> { overflowLoss };
            static StatSet overflowStats = new StatSet(overflowStatList);
            // Unlocking this also doubles your damage
            public static Unlockable OverflowBullets = new UnlockableWithBonus("Overflow Bullets", overflowStats, Damage, 1, true, true, overflowCondition);


            // Lightning!
            public static PlayerStat lightningRange = new PlayerStat("Lightning Arc Range", 300, new Vector2(200, 1000), false, false, 0.25f);
            public static PlayerStat lightningMaxArcs = new PlayerStat("Maximum Arcs", 1, new Vector2(1, 5), true, false);
            public static PlayerStat lightningChainChance = new PlayerStat("Chain Chance", 0.1f, new Vector2(0.1f, 0.8f), false, false);
            static List<PlayerStat> lightningStatList = new List<PlayerStat> { lightningRange, lightningMaxArcs, lightningChainChance };
            static StatSet lightningStats = new StatSet(lightningStatList);
            public static Unlockable Lightning = new Unlockable("Lightning Arc", lightningStats);

            // Laser
            // If you reach high enough shotspeed, piercing and firerate, you get LASER BEAM as an option
            static Condition laserUnlockCondition = new ConjunctCondition(new List<Condition> { new StatCondition(FireRate, 3, true), new StatCondition(ShotSpeed, 1.5f, true), new StatCondition(piercingBulletsPiercingTime, 0.5f, true) });
            public static PlayerStat LaserLifetime = new PlayerStat("Laser Lifetime", 0.2f, new Vector2(0.2f, 2));
            static List<PlayerStat> laserStatList = new List<PlayerStat> { LaserLifetime };
            static StatSet laserStats = new StatSet(laserStatList);
            public static Unlockable Laser = new Unlockable("Laser Beam", laserStats, laserUnlockCondition);

            public static Vector2 LaserSizeVector()
            {
                return new Vector2(5 * BulletSize.GetDynamicVal(), 200 * ShotSpeed.GetDynamicVal());
            }

            // Splinter
            // When the bullet dies, it splits into mini-bullets
            public static PlayerStat splinterFragments = new PlayerStat("Splinter Fragments", 2, new Vector2(2, 5), true);
            public static PlayerStat splinterDamageMultiplier = new PlayerStat("Splinter Damage Fraction", 0.5f, new Vector2(0.5f, 1));
            static List<PlayerStat> splinterStatList = new List<PlayerStat> { splinterFragments, splinterDamageMultiplier };
            static StatSet splinterStats = new StatSet(splinterStatList);
            public static Unlockable Splinter = new Unlockable("Splinter", splinterStats);


            public static Unlockable[] allUnlockables = { Laser, BouncingBullets, PiercingBullets, WallBounce, Lightning, OverflowBullets, Splinter };

        }

        public static class Mutations
        {
            public static AcceleratingBullet AcceleratingBullet = new();
            public static DeceleratingBullet DeceleratingBullet = new();
            public static OrbitBullet OrbitBullet = new();
            public static ChaoticBullet ChaoticBullet = new();
            public static SmartBullet SmartBullet = new();
            public static BoomarangBullet BoomarangBullet = new();

            // The order kind of matters (in the case of having multiple applied). Things that just change speed should go at the back
            public static Mutation[] allMutations = { AcceleratingBullet, DeceleratingBullet, OrbitBullet, ChaoticBullet, SmartBullet, BoomarangBullet };
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

        // The index of the mutation that was chosen last wave
        static int previousMut = -1;
        // Stores the most recent mutation for the wave announcer
        public static string mostRecentMutation;
        public static void IncreaseDifficulty()
        {
            GD.Print("raising difficulty");
            int mutationIndex = GD.RandRange(0, DynamicStats.Length - 1); // only have as many as you have mutation upgrades
            DynamicStats[ID.SpawnRate] += 0.2f;
            if (previousMut != -1)
            {
                // Make sure we don't get the same mutation twice in a row
                while (mutationIndex == previousMut)
                {
                    mutationIndex = GD.RandRange(0, DynamicStats.Length - 1);
                }
            }
            previousMut = mutationIndex;
            mostRecentMutation = string.Format("{0} increased!", ID.nameLookup[mutationIndex]);
            DynamicStats[mutationIndex] *= 1.5f;
        }

    }


    public static class Counters
    {
        public static int WaveCounter = 0;
        public static int KillCounter = 0;
        public static int CoinCounter = 0;

        public static void Reset()
        {
            WaveCounter = 0;
            KillCounter = 0;
            CoinCounter = 0;
        }
    }


    public static void ResetStats()
    {
        Counters.Reset();
        PlayerStats.SetDefaults();
        EnemyStats.SetDefaults();
    }



}
