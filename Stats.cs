using Godot;
using System;

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
        public static float SpawnRate; // How many enemies spawn per second
        public static float HealthMult;
        public static float SpeedMult;
        public static float DropRate; // How likely an enemy is to drop money. If above 100, enemy can drop more than 1 coin


        public static void SetDefaults()
        {
            SpawnRate = 0.5f;
            HealthMult = 1;
            SpeedMult = 1;
            DropRate = 0.2f;
        }

    }


    public static void Reset()
    {
        Player.SetDefaults();
        Enemy.SetDefaults();
    }


}
