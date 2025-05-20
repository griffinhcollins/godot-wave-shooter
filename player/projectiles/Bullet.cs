using Godot;
using static Stats.PlayerStats;
using System.Collections.Generic;
using System.Linq;

public abstract partial class Bullet : Node2D
{

    // [Export]
    // PackedScene splinterShard;

    [Export]
    PackedScene lightningArc;

    protected List<Mutation> currentMutations;

    public bool isShard = false;

    protected bool dead;
    float dmg;

    public float seed { get; private set; }

    protected float timeAlive;

    // Whether the firing sound has finished sounding - can't delete until it has
    bool soundFinished = false;

    protected Vector2 beforePauseVelocity;

    // A bullet can only hit the same mob once
    HashSet<Node2D> mobsHit;

    public Mob shardParent;

    public int numHit { get; private set; }

    // true only if this was sent by the player (not by another bullet)
    public bool originalBullet;

    float shardMult = 1;

    Vector2 initialVelocity;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        dead = false;

        shardMult = isShard ? Unlocks.splinterDamageMultiplier.GetDynamicVal() : 1;
        SetDamage(Damage.GetDynamicVal() * shardMult);
        SetSeed(GD.Randf());

        numHit = 0;
        if (mobsHit is null)
        {
            mobsHit = new HashSet<Node2D>();

        }

        if (this is not LaserBeam)
        {

            GetParent().GetNode<CollisionShape2D>("CollisionShape2D").Scale = Vector2.One * BulletSize.GetDynamicVal() * shardMult;
            GetParent().GetNode<Sprite2D>("Sprite2D").Scale = Vector2.One * BulletSize.GetDynamicVal() * shardMult;
        }

        if (originalBullet)
        {
            GetParent().GetNode<AudioStreamPlayer>("FireSound").Play();

        }
        timeAlive = 0;
        CollisionObject2D parent = GetParent<CollisionObject2D>();

        if (Unlocks.WallBounce.unlocked)
        {
            parent.SetCollisionMaskValue(5, true);

        }

    }


    public void SetSeed(float s)
    {
        if (seed == 0)
        {
            seed = s;

        }
    }
    public float GetTimeAlive()
    {
        return timeAlive;
    }

    public void AddToPosition(Vector2 offset)
    {
        GetParent<Node2D>().Position += offset;
    }


    public void AddToHitMobs(Mob mob)
    {
        if (mobsHit is null)
        {
            mobsHit = new HashSet<Node2D>();
        }
        mobsHit.Add(mob);
    }

    public abstract Vector2 GetCurrentVelocity();
    public virtual void SetVelocity(Vector2 newVelocity, bool normalize = true)
    {
        if (initialVelocity == Vector2.Zero)
        {
            initialVelocity = newVelocity;
        }
    }
    public Vector2 GetInitialVelocity()
    {
        if (initialVelocity == Vector2.Zero)
        {
            initialVelocity = GetCurrentVelocity();
        }
        return initialVelocity;
    }
    protected abstract void Pause();
    protected abstract void UnPause();

    public override void _Process(double delta)
    {
        if (State.currentState == State.paused)
        {
            if (beforePauseVelocity == Vector2.Zero)
            {
                beforePauseVelocity = GetCurrentVelocity();
                Pause();
            }
            return;
        }
        if (beforePauseVelocity != Vector2.Zero)
        {
            UnPause();
            beforePauseVelocity = Vector2.Zero;

        }

        timeAlive += (float)delta;

        foreach (Mutation m in GetMutations())
        {
            m.OngoingEffect(delta, this);
        }

    }

    List<Mutation> GetMutations()
    {
        if (currentMutations is null)
        {
            return new List<Mutation> { };
        }
        else
        {
            return currentMutations;
        }
    }

    public void AddMutation(Mutation mut)
    {
        if (currentMutations is null)
        {
            currentMutations = new List<Mutation> { mut };
        }
        else
        {
            currentMutations.Add(mut);
        }
        mut.ImmediateEffect(this);
    }


    // The bullet's damage can change after certain events
    public void SetDamage(float newDmg)
    {

        dmg = newDmg;
    }

    private void OnCollision(Node2D body)
    {

        // This always happens when we hit something, regardless of if it is the final hit
        HandleCollision(body);
        //  Now do checks for things that are only on final or non-final hit
        if (
            ((numHit <= Unlocks.bouncingBulletBounces.GetDynamicVal()) || // Don't die if we have bounces left
            (timeAlive < Unlocks.piercingBulletsPiercingTime.GetDynamicVal()) || // Don't die if we have piercing left
            Unlocks.OverflowBullets.unlocked) && // Don't die if overflow bullets are unlocked
            dmg > 0 // Do die if we've run out of damage, even if we have bounces/pierces left
         )
        {
            // Not the final hit - do stuff that only triggers on intermediate hits
        }
        else
        {
            if (isShard && body == shardParent)
            {
                return;
            }
            // Final hit
            HandleDeath(body);

        }
    }

    protected virtual void HandleCollision(Node2D hitNode)
    {

        if (hitNode.IsInGroup("border"))
        {
            numHit++;
            // If a piercing shot hits a wall while wall bounces are unlocked, instantly convert it into bouncy
            if (Unlocks.piercingBulletsPiercingTime.GetDynamicVal() > 0)
            {
                timeAlive = Unlocks.piercingBulletsPiercingTime.GetDynamicVal();
            }
            // Reduce damage of bullet when it hits a border
            SetDamage(dmg * Unlocks.wallBounceDamageRetention.GetDynamicVal());
        }
        // Activate any abilities that trigger on hit
        if (hitNode is Mob)
        {
            Mob hitMob = (Mob)hitNode;
            if (Unlocks.Lightning.unlocked)
            {

                SpawnLightning(hitMob);
            }
            if (Unlocks.OverflowBullets.unlocked)
            {
                SetDamage(dmg - Unlocks.overflowLoss.GetDynamicVal() * hitMob.GetHP());
            }
            if (Unlocks.Splinter.unlocked && Unlocks.Laser.unlocked)
            {
                Splinter(hitNode);
            }
        }

        foreach (Mutation m in GetMutations())
        {
            m.OnCollision(this);
        }

        // Finally, actually deal damage
        if (hitNode.IsInGroup("mobs") && !mobsHit.Contains(hitNode))
        {
            numHit++;
            mobsHit.Add(hitNode);
            Mob mobHit = (Mob)hitNode;
            mobHit.TakeDamage(dmg);
        }


    }


    // returns all the mobs that got hit so other branches don't hit the same mobs
    HashSet<Mob> SpawnLightning(Mob seedMob, HashSet<Mob> alreadyHit = null)
    {
        if (alreadyHit is null)
        {
            alreadyHit = new HashSet<Mob> { seedMob };
        }
        else
        {
            alreadyHit.Add(seedMob);
        }
        float arcRange = Unlocks.lightningRange.GetDynamicVal();
        List<Mob> targets = new List<Mob>();
        // Find the nearest mob
        foreach (Mob mob in GetTree().GetNodesInGroup("mobs"))
        {
            if (alreadyHit.Contains(mob))
            {
                continue;
            }

            if ((mob.GlobalPosition - seedMob.GlobalPosition).LengthSquared() < arcRange * arcRange)
            {
                targets.Add(mob);
            }
        }

        if (targets.Count > 0)
        {
            targets.OrderBy(t => (t.GlobalPosition - seedMob.GlobalPosition).LengthSquared());

            for (int i = 0; i < Mathf.Min(Unlocks.lightningMaxArcs.GetDynamicVal(), targets.Count); i++)
            {
                Mob target = targets[i];
                Node2D arc = lightningArc.Instantiate<Node2D>();
                Line2D line = arc.GetNode<Line2D>("Arc");
                GetTree().Root.AddChild(arc);
                line.AddPoint(seedMob.GlobalPosition);
                line.AddPoint(target.GlobalPosition);

                target.TakeDamage(dmg);
                if (Unlocks.lightningChainChance.GetDynamicVal() > GD.Randf())
                {
                    HashSet<Mob> hits = SpawnLightning(target, alreadyHit);
                    alreadyHit.UnionWith(hits);
                }
            }
        }
        return alreadyHit;
    }

    protected virtual void HandleDeath(Node2D lastHit = null, bool splinterOveride = true)
    {
        if (dead)
        {
            return;
        }

        if (Unlocks.Splinter.unlocked && splinterOveride && !isShard && !Unlocks.Laser.unlocked)
        {

            Splinter(lastHit);
        }


        dead = true;
        Node2D parent = GetParent<Node2D>();
        parent.Hide();
        ((CollisionObject2D)parent).CollisionLayer = 0; // Disable collision
        ((CollisionObject2D)parent).CollisionMask = 0; // Disable collision
        if (soundFinished)
        {
            parent.QueueFree();

        }
    }

    protected void Splinter(Node2D lastHit = null, int shardCountOveride = -1)
    {
        Mob hitMob = null;
        if (lastHit is not null && lastHit is Mob)
        {
            hitMob = (Mob)lastHit;
        }

        Node2D parent = GetParent<Node2D>();
        int shards = (int)Unlocks.splinterFragments.GetDynamicVal();
        if (shardCountOveride != -1)
        {
            shards = shardCountOveride;
        }
        for (int i = 0; i < shards; i++)
        {
            // Copy myself
            Node2D shardBody = (GD.Load(GetParent().SceneFilePath) as PackedScene).Instantiate<Node2D>();
            Bullet shard = shardBody.GetNode<Bullet>("ScriptHolder");
            if (this is LaserBeam)
            {
                // If splinter is with laser beam, spawns a laser beam off a hit enemy
                shardBody.GlobalPosition = lastHit.GlobalPosition;
                // Make it perpendicular in either direction
                shardBody.Rotation = GD.Randf() * 2 * Mathf.Pi;
            }
            else
            {
                shardBody.Position = parent.Position;
                Vector2 shardVelocity = GetCurrentVelocity().Rotated((GD.Randf() + 1 / 6) * 1.5f * Mathf.Pi);
                shard.SetVelocity(shardVelocity);

            }
            shard.isShard = true;
            if (hitMob is not null)
            {
                shard.AddToHitMobs(hitMob);
                shard.shardParent = hitMob;
            }
            GetTree().Root.CallDeferred(MethodName.AddChild, shardBody);


        }
    }

    private void SoundFinished()
    {
        soundFinished = true;
        if (dead)
        {
            QueueFree();

        }
    }


}
