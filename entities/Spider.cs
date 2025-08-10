public partial class Spider : Fish
{



    protected override string GetMobName()
    {
        return "Spider";
    }

    protected override float GetIrisMoveRadius()
    {
        return 10;
    }

    protected override float GetPupilMoveRadius()
    {
        return 5;
    }
}