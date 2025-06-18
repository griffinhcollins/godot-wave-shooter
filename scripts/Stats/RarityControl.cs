using Godot;

public static class RarityControl
{


    public class Rarity
    {
        public string name;
        public int strength;
        public Color colour;

        public Rarity(string _name, int _strength, Color _colour)
        {
            name = _name;
            strength = _strength;
            colour = _colour;
        }
    }

    public static Rarity Common = new Rarity("Common", 0, new Color(1, 1, 1));
    public static Rarity Uncommon = new Rarity("Uncommon", 1, new Color(0.8f, 0.8f, 1));
    public static Rarity Rare = new Rarity("Rare", 2, new Color(0.8f, 0.4f, 1f));
    public static Rarity Legendary = new Rarity("Legendary", 3, new Color(1, 0.8f, 0)); // All unlocks are legendary
    public static Rarity NotFoundInShops = new Rarity("Not Applicable (never appears in shops)", -1, new Color(0, 0, 0));
}