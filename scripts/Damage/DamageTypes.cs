public static class DamageTypes
{
    public static DamageType Blunt = new DamageType("Blunt");
    public static DamageType Sharp = new DamageType("Sharp");
    public static DamageType Poison = new DamageType("Poison");
    public static DamageType Fire = new DamageType("Fire");
    public static DamageType Electric = new DamageType("Electric");
    public static DamageType Laser = new DamageType("Laser");


    public static DamageType[] damageTypes = { Blunt, Sharp, Poison, Fire, Electric, Laser };
}