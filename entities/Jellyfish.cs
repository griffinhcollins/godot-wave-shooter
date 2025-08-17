public partial class Jellyfish : Fish
{

    protected override float GetIrisMoveRadius()
    {
        return 0;
    }

    public override string GetMobName()
    {
        return "Jellyfish";
    }


    protected override float GetPupilMoveRadius()
    {
        return 10;
    }
}