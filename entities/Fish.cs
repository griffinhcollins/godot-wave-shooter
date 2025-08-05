using Godot;

using static Stats.EnemyStats;
using static Stats.PlayerStats.Unlocks;
public abstract partial class Fish : Organic 
{
    // The starting basic mobs that kind of accelerate towards the player but also just kinda drift around
    // Notable for gazing!

    float baseSpeedLimit = 400;
    float baseAcceleration = 1;

    Node2D eye;

    float speedLimit;
    float acceleration;



    public override void _Ready()
    {
        base._Ready();


        acceleration = baseAcceleration * DynamicStats[ID.AccelerationMult];
        speedLimit = baseSpeedLimit;
        eye = GetNode<Node2D>("Eye");
        eye.Scale *= size;

    }


    
    protected abstract float GetIrisMoveRadius();
    protected abstract float GetPupilMoveRadius();

    protected override void ProcessMovement(double delta)
    {
        // Point towards the player
        ApplyForce((player.Position - Position) * ((player.Position - Position).Length() * 1 / 1000 + speedLimit / 500) * acceleration);
        ApplyTorque(LinearVelocity.AngleTo(ToGlobal(Vector2.Up)) * 1000);
        float currentSpeedSq = LinearVelocity.LengthSquared();
        if (currentSpeedSq > speedLimit * speedLimit)
        {
            LinearVelocity = LinearVelocity.Normalized() * speedLimit;
            // ApplyForce(LinearVelocity * (speedLimit - Mathf.Pow(currentSpeedSq, 0.5f)));
        }

        GazeAt(player.GlobalPosition, (float)delta);
    }



    void GazeAt(Vector2 targetPos, float delta)
    {

        float irisMoveRadius = GetIrisMoveRadius();
        float pupilMoveRadius = GetPupilMoveRadius();

        eye.GlobalPosition = eye.GlobalPosition.Lerp((targetPos - GlobalPosition).Normalized() * irisMoveRadius + GlobalPosition, delta * 3);


        Node2D pupil = eye.GetNode<Node2D>("Pupil");


        pupil.GlobalPosition = pupil.GlobalPosition.Lerp((targetPos - eye.GlobalPosition).Normalized() * pupilMoveRadius + eye.GlobalPosition, delta * 5);

        // float oldRot = eye.Rotation;
        // eye.LookAt(targetPos);
        // float targetRot = eye.Rotation;
        // eye.Rotation = Mathf.Lerp(oldRot, targetRot, delta * 10);
    }


}