
public class Unlockable{
    public bool unlocked = false;
    public string name;

    public Condition condition;

    public Unlockable(string _name, Condition _condition = null){
        name = _name;
        condition = _condition;
    } 






}