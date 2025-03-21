using Godot;
using System;
using System.Runtime.CompilerServices;

public static class Stats
{

    // Player attributes
    public static class Player
    {
        public static float BaseDamage; // Default damage per shot
        public static float FiringSpeed; // Shots/second
        public static int HP; // Player HP
        public static int HPReward; // How much extra money the player gets per remaining HP at the end of a wave



        public static void SetDefaults()
        {
            BaseDamage = 1;
            FiringSpeed = 2;
            HP = 3;
            HPReward = 2;
        }
    }


    // Enemy attributes
    public static class Enemy
    {
        public static int WaveLength;
        public static float SpawnRate; // How many enemies spawn per second
        public static float HealthMult;
        public static float SpeedMult;
        public static float DropRate; // How likely an enemy is to drop money. If above 100, enemy can drop more than 1 coin


        public static void SetDefaults()
        {
            WaveLength = 20;
            SpawnRate = 1;
            HealthMult = 1;
            SpeedMult = 1;
            DropRate = 1.5f;
        }

    }


    public static class Upgrades{
        public static class ID{
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
        

        public static Texture2D GetUpgradeIcon(int statChangeID){
            
            string[] paths = {"dmg_up.png", "dmg_down.png", "firerate_up.png", "firerate_down.png", "hp_up.png", "hp_down.png", "hpreward_up.png", "hpreward_down.png", "moneyrate_up.png", "moneyrate_down.png"};
            return (Texture2D)GD.Load("res://custom assets/upgrade icons/" + paths[statChangeID]);
        }


        public static void ExecuteUpgrade(int ID, float magnitude){
            // These are exponential at the moment, but that's kinda funny
            GD.Print(string.Format("ID {0} affected with magnitude {1}", ID, magnitude));
            switch(ID){
                case 0:
                    Player.BaseDamage += magnitude * Player.BaseDamage;
                    break;
                case 1:
                    Player.BaseDamage -= magnitude * Player.BaseDamage;
                    break;
                case 2:
                    Player.FiringSpeed += magnitude * Player.FiringSpeed;
                    break;
                case 3:
                    Player.FiringSpeed -= magnitude * Player.FiringSpeed;
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
                    Enemy.DropRate += magnitude * Enemy.DropRate;
                    break;
                case 9:
                    Enemy.DropRate -= magnitude * Enemy.DropRate;
                    break;
            }
        }


    }


    public static void Reset()
    {
        Player.SetDefaults();
        Enemy.SetDefaults();
    }


}
