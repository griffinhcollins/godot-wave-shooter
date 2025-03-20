using Godot;

public partial class Mob : RigidBody2D
{

    Player player;


    float speedLimit = 500;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

        player = (Player)GetTree().GetNodesInGroup("player")[0];

        AnimatedSprite2D animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        string[] mobTypes = animSprite.SpriteFrames.GetAnimationNames();
        animSprite.Play(mobTypes[GD.Randi() % mobTypes.Length]);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Point towards the player
        ApplyForce(player.Position - Position);
        ApplyTorque(LinearVelocity.AngleTo(ToGlobal(Vector2.Up)) * 1000);
        float currentSpeedSq = LinearVelocity.LengthSquared();
        if (currentSpeedSq > speedLimit * speedLimit)
        {
            ApplyForce(LinearVelocity * (speedLimit - Mathf.Pow(currentSpeedSq, 0.5f)));
        }


    }

    private void OnScreenExit()
    {
        QueueFree();
    }
}
