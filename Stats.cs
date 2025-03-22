using Godot;
using System;
using System.Runtime.CompilerServices;

public static class Stats
{

    // Player attributes
    public static class Player
    {
        // Starting/default stats (what is used to calculate upgrades)
        public static readonly float BaseDamage = 1; // Default damage per shot
        public static readonly float BaseFiringSpeed = 2; // Shots/second
        public static readonly int BaseHP = 3; // Player HP
        public static readonly int BaseHPReward = 2; // How much extra money the player gets per remaining HP at the end of a wave



        // Current stats
        public static float Damage; // Default damage per shot
        public static float FiringSpeed; // Shots/second
        public static int HP; // Player HP
        public static int HPReward; // How much extra money the player gets per remaining HP at the end of a wave



        public static void SetDefaults()
        {
            Damage = BaseDamage;
            FiringSpeed = BaseFiringSpeed;
            HP = BaseHP;
            HPReward = BaseHPReward;
        }



        



        

    }


    // Enemy attributes
    public static class Enemy
    {

        // Starting/default stats (what is used to calculate upgrades)
        public static readonly int BaseWaveLength = 15;
        public static readonly float BaseSpawnRate = 1; // How many enemies spawn per second
        public static readonly float BaseHealthMult = 1;
        public static readonly float BaseSpeedMult = 1;
        public static readonly float BaseDropRate = 1.5f; // How likely an enemy is to drop money. If above 100, enemy can drop more than 1 coin


        // Dynamic stats
        public static int WaveLength;
        public static float SpawnRate; // How many enemies spawn per second
        public static float HealthMult;
        public static float SpeedMult;
        public static float DropRate; // How likely an enemy is to drop money. If above 100, enemy can drop more than 1 coin


        public static void SetDefaults()
        {
            WaveLength = BaseWaveLength;
            SpawnRate = BaseSpawnRate;
            HealthMult = BaseHealthMult;
            SpeedMult = BaseSpeedMult;
            DropRate = BaseDropRate;
        }

    }


    public static class Upgrades
    {
        public static class ID
        {
            public static readonly int numUpgrades = 10;
            // Evens are positive, negatives are odd
            public static readonly int dmgUp = 0;
            public static readonly int dmgDown = 1;
            public static readonly int firerateUp = 2;
            public static readonly int firerateDown = 3;
            public static readonly int hpUp = 4;
            public static readonly int hpDown = 5;
            public static readonly int hpRewardUp = 6;
            public static readonly int hpRewardDown = 7;
            public static readonly int droprateUp = 8;
            public static readonly int droprateDown = 9;
        }


        public static Texture2D GetUpgradeIcon(int statChangeID)
        {

            string[] paths = { "dmg_up.png", "dmg_down.png", "firerate_up.png", "firerate_down.png", "hp_up.png", "hp_down.png", "hpreward_up.png", "hpreward_down.png", "moneyrate_up.png", "moneyrate_down.png" };
            return (Texture2D)GD.Load("res://custom assets/upgrade icons/" + paths[statChangeID]);
        }


        public static void ExecuteUpgrade(int ID, float magnitude)
        {
            
            switch (ID)
            {
                case 0:
                    Player.Damage += magnitude * Player.BaseDamage;
                    break;
                case 1:
                    Player.Damage -= magnitude * Player.BaseDamage;
                    break;
                case 2:
                    Player.FiringSpeed += magnitude * Player.BaseFiringSpeed;
                    break;
                case 3:
                    Player.FiringSpeed -= magnitude * Player.BaseFiringSpeed;
                    break;
                case 4:
                    Player.HP += (int)magnitude;
                    break;
                case 5:
                    Player.HP -= (int)magnitude;
                    break;
                case 6:
                    Player.HPReward += (int)magnitude;
                    break;
                case 7:
                    Player.HPReward -= (int)magnitude;
                    break;
                case 8:
                    Enemy.DropRate += magnitude * Enemy.BaseDropRate;
                    break;
                case 9:
                    Enemy.DropRate -= magnitude * Enemy.BaseDropRate;
                    break;
            }
            PrintStats();
        }


    }


    public static void Reset()
    {
        Player.SetDefaults();
        Enemy.SetDefaults();
    }

    public static void PrintStats(){
            GD.Print(string.Format("Damage = {0}", Player.Damage));
            GD.Print(string.Format("FiringSpeed = {0}", Player.FiringSpeed));
            GD.Print(string.Format("HP = {0}", Player.HP));
            GD.Print(string.Format("HPReward = {0}", Player.HPReward));
            GD.Print(string.Format("DropRate = {0}", Enemy.DropRate));
        }


}
