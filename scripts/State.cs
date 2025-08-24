using Godot;
using System;

public static class State
{

    public static PackedSceneHolder sceneHolder;
    public static Hud hud;

    public static Improvement MobDamage; // exists as a source for visual effects

    public static int difficulty;

    public static int EASY = 0;
    public static int MEDIUM = 1;
    public static int HARD = 2;



    public static int currentState;
    public static int alive = 0;
    public static int shop = 1;
    public static int dead = 2;
    public static int paused = 3;
    public static int mainmenu = 4;

    public static int unPauseState;
}
