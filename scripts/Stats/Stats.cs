using Godot;
using System.Collections.Generic;
using System.Linq;
using static RarityControl;

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

        public static PlayerStat Damage = new PlayerStat("Damage", 10, new Vector2(5, Mathf.Inf), Common);
        public static PlayerStat FireRate = new PlayerStat("Firerate", 2, new Vector2(0.5f, 10), Common);
        public static PlayerStat MaxHP = new PlayerStat("HP", 3, new Vector2(0, 6), Rare, true);
        public static PlayerStat HPReward = new PlayerStat("HP Interest", 1, new Vector2(0, 4), Rare, true);
        public static PlayerStat Multishot = new PlayerStat("Multishot", 1, new Vector2(1, 10), Uncommon);
        public static PlayerStat Spread = new PlayerStat("Spread", 15, new Vector2(0, 180), Common, false, true);
        public static PlayerStat ShotSpeed = new PlayerStat("Shot Speed", 1, new Vector2(0.25f, 10), Common);
        public static PlayerStat Speed = new PlayerStat("Speed", 400, new Vector2(100, 1000), Uncommon, false, false, 0.5f);
        public static PlayerStat DamageRecoil = new PlayerStat("Damage Recoil Strength", 1, new Vector2(1, 5), Rare);
        public static PlayerStat RevengeDamage = new PlayerStat("Revenge Damage", 0, new Vector2(0, 3), Rare);
        public static PlayerStat BulletSize = new PlayerStat("Bullet Size", 0.7f, new Vector2(0.25f, 3), Common, false, false, 0.75f);
        public static PlayerStat Lifetime = new PlayerStat("Bullet Lifetime", 1, new Vector2(1, 20), Uncommon, false, false, 1.5f);
        public static PlayerStat DropRate = new PlayerStat("Drop Rate", 1.5f, new Vector2(0.25f, 4), Rare);
        public static PlayerStat MoneyCap = new PlayerStat("Money Cap", 50, new Vector2(50, Mathf.Inf), Rare, true, false, 50, new CounterCondition(Counters.WaveCounter, 10)); // Only unlock after 10 waves have been beaten
        static List<PlayerStat> defaultStatList = new List<PlayerStat> { Damage, FireRate, MaxHP, HPReward, Multishot, Spread, ShotSpeed, Speed, DamageRecoil, RevengeDamage, BulletSize, Lifetime, DropRate, MoneyCap };

        static StatSet defaultStats = new StatSet(defaultStatList);

        public static List<PlayerStat> GetDefaultStats()
        {
            return defaultStatList;
        }

        // Misc. Stats (not put into statsets because they don't have a combined condition or upgrades in the shop)
        public static int Money;
        public static PlayerStat UpgradeSlots = new PlayerStat("Upgrade Slots", 3, new Vector2(3, 7), NotFoundInShops, true);
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
            Money = 0;
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
            public static PlayerStat bouncingBulletBounces = new PlayerStat("Bounces", 1, new Vector2(0, 10), Common, true);
            static List<PlayerStat> bouncingBulletsStatList = new List<PlayerStat> { bouncingBulletBounces };
            static StatSet bouncingBulletsStats = new StatSet(bouncingBulletsStatList);
            public static Unlockable BouncingBullets = new Unlockable("Bouncing Bullets", bouncingBulletsStats);


            // Wallbounce, it's a direct upgrade to bouncing bullets
            static Condition wallBounceUnlockCondition = new ConjunctCondition(new List<Condition> { new StatCondition(bouncingBulletBounces, 2, true), new UnlockCondition(BouncingBullets, true) });
            public static PlayerStat wallBounceDamageRetention = new PlayerStat("Wall Bounce Damage Retention", 0.5f, new Vector2(0.5f, 1), Uncommon, false, false, 0.5f);
            static List<PlayerStat> wallBounceStatList = new List<PlayerStat> { wallBounceDamageRetention };
            static StatSet wallBounceStats = new StatSet(wallBounceStatList, wallBounceUnlockCondition);
            public static Unlockable WallBounce = new Unlockable("Wall Bounce", wallBounceStats, null, wallBounceUnlockCondition);

            // Piercing
            public static PlayerStat piercingBulletsPiercingTime = new PlayerStat("Piercing Time", 0.25f, new Vector2(0, 2), Common, false, false, 0.5f);
            static List<PlayerStat> piercingBulletsStatList = new List<PlayerStat> { piercingBulletsPiercingTime };
            static StatSet piercingBulletsStats = new StatSet(piercingBulletsStatList);
            public static Unlockable PiercingBullets = new Unlockable("Piercing Bullets", piercingBulletsStats);

            // Piercing overwhammer style
            static Condition overflowCondition = new ConjunctCondition(new List<Condition> { new StatCondition(piercingBulletsPiercingTime, 0.5f, true), new StatCondition(Damage, 20, true) });

            // What fraction of the damage is reduced when hitting an enemy, e.g. if the enemy has 10 health and this stat is 0.5f, the overflow loses 5 damage
            public static PlayerStat overflowLoss = new PlayerStat("Overflow Reduction", 1, new Vector2(0, 1), Uncommon, false, true);
            static List<PlayerStat> overflowStatList = new List<PlayerStat> { overflowLoss };
            static StatSet overflowStats = new StatSet(overflowStatList);
            // Unlocking this also doubles your damage
            public static Unlockable OverflowBullets = new UnlockableWithBonus("Overflow Bullets", overflowStats, false, null, Damage, 1, true, true, overflowCondition);


            // Lightning!
            public static PlayerStat lightningRange = new PlayerStat("Lightning Arc Range", 300, new Vector2(200, 1000), Rare, false, false, 0.5f);
            public static PlayerStat lightningMaxArcs = new PlayerStat("Maximum Arcs", 1, new Vector2(1, 5), Uncommon, true, false);
            public static PlayerStat lightningChainChance = new PlayerStat("Chain Chance", 0.1f, new Vector2(0.1f, 0.8f), Common, false, false);
            public static PlayerStat lightningChainDamageRetention = new PlayerStat("Chain Damage Retention", 0.5f, new Vector2(0.5f, 1), Common, false, false, 0.5f, new StatCondition(lightningChainChance, 0.2f, true));
            static List<PlayerStat> lightningStatList = new List<PlayerStat> { lightningRange, lightningMaxArcs, lightningChainChance, lightningChainDamageRetention };
            static StatSet lightningStats = new StatSet(lightningStatList);
            public static Unlockable Lightning = new Unlockable("Lightning Arc", lightningStats, new List<VisualEffect> { new ParticleEffect(Lightning, "Lightning Arc") });

            // returns all the mobs that got hit so other branches don't hit the same mobs
            public static HashSet<Mob> SpawnLightning(float dmg, Mob seedMob, int depth, PackedScene arcScene, HashSet<Mob> alreadyHit = null)
            {
                if (alreadyHit is null)
                {
                    alreadyHit = new HashSet<Mob> { seedMob };
                }
                else
                {
                    alreadyHit.Add(seedMob);
                }
                float arcRange = Unlocks.lightningRange.GetDynamicVal();
                List<Mob> targets = new List<Mob>();
                // Find the nearest mob
                foreach (Mob mob in seedMob.GetTree().GetNodesInGroup("mobs"))
                {
                    if (alreadyHit.Contains(mob))
                    {
                        continue;
                    }

                    if ((mob.GlobalPosition - seedMob.GlobalPosition).LengthSquared() < arcRange * arcRange)
                    {
                        targets.Add(mob);
                    }
                }

                if (targets.Count > 0)
                {
                    targets.OrderBy(t => (t.GlobalPosition - seedMob.GlobalPosition).LengthSquared());

                    for (int i = 0; i < Mathf.Min(Unlocks.lightningMaxArcs.GetDynamicVal(), targets.Count); i++)
                    {
                        Mob target = targets[i];
                        Node2D arc = arcScene.Instantiate<Node2D>();
                        Line2D line = arc.GetNode<Line2D>("Arc");
                        target.GetTree().Root.AddChild(arc);
                        line.AddPoint(seedMob.GlobalPosition);
                        line.AddPoint(target.GlobalPosition);

                        target.TakeDamage(dmg * Mathf.Pow(Unlocks.lightningChainDamageRetention.GetDynamicVal(), depth));
                        if (Unlocks.lightningChainChance.GetDynamicVal() > GD.Randf())
                        {
                            HashSet<Mob> hits = SpawnLightning(dmg, target, depth + 1, arcScene, alreadyHit);
                            alreadyHit.UnionWith(hits);
                        }
                    }
                }
                return alreadyHit;
            }

            // Laser
            // If you reach high enough shotspeed, piercing and firerate, you get LASER BEAM as an option
            static Condition laserUnlockCondition = new ConjunctCondition(new List<Condition> { new StatCondition(FireRate, 3, true), new StatCondition(ShotSpeed, 1.5f, true), new StatCondition(piercingBulletsPiercingTime, 0.5f, true) });
            public static PlayerStat LaserLifetime = new PlayerStat("Laser Lifetime", 0.2f, new Vector2(0.2f, 2), Uncommon);
            static List<PlayerStat> laserStatList = new List<PlayerStat> { LaserLifetime };
            static StatSet laserStats = new StatSet(laserStatList);
            public static Unlockable Laser = new Unlockable("Laser Beam", laserStats, null, laserUnlockCondition);

            public static Vector2 LaserSizeVector()
            {
                return new Vector2(5 * BulletSize.GetDynamicVal(), 200 * ShotSpeed.GetDynamicVal());
            }

            // Splinter
            // When the bullet dies, it splits into mini-bullets
            public static PlayerStat splinterFragments = new PlayerStat("Splinter Fragments", 2, new Vector2(2, 5), Common, true);
            public static PlayerStat splinterDamageMultiplier = new PlayerStat("Splinter Damage Fraction", 0.5f, new Vector2(0.5f, 1), Uncommon, false, false, 0.5f);
            static List<PlayerStat> splinterStatList = new List<PlayerStat> { splinterFragments, splinterDamageMultiplier };
            static StatSet splinterStats = new StatSet(splinterStatList);
            public static Unlockable Splinter = new Unlockable("Splinter", splinterStats);


            // Venom
            // Deals DoT, independent of damage
            public static PlayerStat venomFrequency = new PlayerStat("Venom Frequency", 1, new Vector2(1, 8), Common, true);
            public static PlayerStat venomDamage = new PlayerStat("Venom Damage", 2, new Vector2(2, 20), Common);
            static List<PlayerStat> venomStatList = new List<PlayerStat> { venomFrequency, venomDamage };
            static StatSet venomStats = new StatSet(venomStatList);
            public static Unlockable Venom = new Unlockable("Venom", venomStats, new List<VisualEffect> { new StaticColourChange(Venom, Colors.Green, 5, 4), new ParticleEffect(Venom, "Venom") });


            // Plague
            // Requires venom, makes mobs spread poison to each other if they take a venom tick while touching

            static List<PlayerStat> plagueStatList = new List<PlayerStat> { };
            static StatSet plagueStats = new StatSet(plagueStatList);
            public static Unlockable Plague = new Unlockable("Plague", plagueStats, new List<VisualEffect> { new StaticColourChange(Plague, Colors.RebeccaPurple, 1, 5, Mathf.Inf, new List<Improvement> { Venom }) }, new StatCondition(venomFrequency, 3, true));

            // Plague upgrade: exploding corpses
            public static PlayerStat plagueExplosionRadius = new PlayerStat("Plague Explosion Radius", 40, new Vector2(40, 200), Common);
            public static PlayerStat plagueExplosionChance = new PlayerStat("Plague Explosion Chance", 0.1f, new Vector2(0.1f, 0.5f), Common, false, false, 0.5f);
            public static PlayerStat plagueCloudLifetime = new PlayerStat("Plague Cloud Lifetime", 0.5f, new Vector2(0.5f, 2), Uncommon, false, false, 0.5f);
            static List<PlayerStat> plagueExplosionStatList = new List<PlayerStat> { plagueExplosionRadius, plagueExplosionChance, plagueCloudLifetime };
            static StatSet plagueExplosionStats = new StatSet(plagueExplosionStatList);
            public static Unlockable PlagueExplosion = new Unlockable("Plague Explosion", plagueExplosionStats, null, new UnlockCondition(Plague, true));

            // If you have both lightning and plague, venom ticks can sometimes trigger a lightning bolt
            public static PlayerStat lightningPlagueChance = new PlayerStat("Lightning Plague Chance", 0.2f, new Vector2(0.2f, 0.5f), Rare, false, false, 0.5f);
            static List<PlayerStat> lightningPlagueStatList = new List<PlayerStat> { lightningPlagueChance };
            static StatSet lightningPlagueStats = new StatSet(lightningPlagueStatList);
            public static Unlockable LightningPlague = new Unlockable("Lightning Plague", lightningPlagueStats, null,
                new ConjunctCondition(new List<Condition> { new UnlockCondition(Lightning, true), new UnlockCondition(Plague, true) }));


            // Explosions on death
            public static PlayerStat explosionRadius = new PlayerStat("Explosion Radius", 50, new Vector2(50, 200), Rare);
            public static PlayerStat explosionChance = new PlayerStat("Chance of Explosion on Death", 0.3f, new Vector2(0.3f, 0.8f), Rare, false, false, 0.5f);
            public static PlayerStat explosionDamage = new PlayerStat("Explosion Damage", 20, new Vector2(20, Mathf.Inf), Common);

            static List<PlayerStat> explosionStatList = new List<PlayerStat> { explosionChance, explosionDamage, explosionRadius };
            static StatSet explosionStats = new StatSet(explosionStatList);
            public static Unlockable DeathExplosion = new Unlockable("Death Explosion", explosionStats, new List<VisualEffect> { new ParticleEffect(DeathExplosion, "death explosion"), new StaticColourChange(Plague, Colors.Red, 5, 3) });


            // Shield that regenerates after a period of time
            public static PlayerStat shieldRecharge = new PlayerStat("Shield Cooldown", 10, new Vector2(2, 10), Rare, false, true, 0.5f);
            static List<PlayerStat> shieldStatList = new List<PlayerStat> { shieldRecharge };
            static StatSet shieldStats = new StatSet(shieldStatList);
            public static Unlockable Shield = new Unlockable("Shield", shieldStats);

            public static Unlockable[] allUnlockables = { Laser, BouncingBullets, PiercingBullets, WallBounce, Lightning, OverflowBullets, Splinter, Venom, Plague, PlagueExplosion, LightningPlague, DeathExplosion, Shield };


        }

        public static class Mutations
        {
            public static AcceleratingBullet AcceleratingBullet = new();
            public static DeceleratingBullet DeceleratingBullet = new();
            public static OrbitBullet OrbitBullet = new();
            public static ChaoticBullet ChaoticBullet = new();
            public static SmartBullet SmartBullet = new();
            public static BoomarangBullet BoomarangBullet = new();
            public static GrowingBullet GrowingBullet = new();

            // The order kind of matters (in the case of having multiple applied). Things that just change speed should go at the back
            public static Mutation[] allMutations = { AcceleratingBullet, DeceleratingBullet, OrbitBullet, ChaoticBullet, SmartBullet, BoomarangBullet, GrowingBullet };

            static Mutation currentMutation;

            public static List<Mutation> GetAvailableMutations() // returns all mutations that aren't active
            {
                if (currentMutation is null)
                {
                    return allMutations.ToList();
                }
                return allMutations.Where(m => m != currentMutation).ToList();
            }

            public static void SetMutation(Mutation mut)
            {
                if (currentMutation is not null)
                {
                    currentMutation.applied = false;

                }
                mut.applied = true;
                currentMutation = mut;
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
                float difficultyMult = 1;
                if (i == 1)
                {
                    // spawn rate
                    switch (State.difficulty)
                    {
                        case 0:
                            difficultyMult = 0.6f;
                            break;
                        case 1:
                            difficultyMult = 1;
                            break;
                        case 2:
                            difficultyMult = 1.25f;
                            break;
                        default:
                            GD.Print("weird difficulty");
                            break;
                    }
                }
                DynamicStats[i] = BaseStats[i] * difficultyMult;
            }
        }

        // The index of the mutation that was chosen last wave
        static int previousMut = -1;
        // Stores the most recent mutation for the wave announcer
        public static string mostRecentMutation;
        public static void IncreaseDifficulty()
        {
            // GD.Print("raising difficulty");
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
        public class Counter
        {
            public string Name;
            public int Value;
            public int Default;

            public Counter(string _name, int _default)
            {
                Name = _name;
                Default = _default;
                Value = _default;
            }

            public void Reset()
            {
                Value = Default;
            }
        }
        public static Counter WaveCounter = new Counter("Wave", 1);
        public static Counter KillCounter = new Counter("Kills", 0);
        public static Counter CoinCounter = new Counter("Coins", 0);

        static List<Counter> allCounters = new List<Counter> { WaveCounter, KillCounter, CoinCounter };

        public static void Reset()
        {
            foreach (Counter c in allCounters)
            {
                c.Reset();
            }
        }

        public static bool IsUnlockWave()
        {
            return WaveCounter.Value % 5 == 1;
        }
    }


    public static void ResetStats()
    {
        Counters.Reset();
        PlayerStats.SetDefaults();
        EnemyStats.SetDefaults();
    }



}
