using Godot;
using static Stats.PlayerStats;
using System.Collections.Generic;
using System.Linq;

public partial class Bullet : IAffectedByVisualEffects
{

    // [Export]
    // PackedScene splinterShard;

    public string name;
    public Vector2 direction;
    public Vector2 position;
    public float lifetime = 0;
    public float speed;
    public int imageIndex;
    public Rid shapeID;
    public Rid body;

    public bool isLaser = false;


    protected DamageType damageType;

    protected List<Mutation> currentMutations;

    public bool isShard = false;

    protected bool dead;
    float dmg;


    public float seed { get; private set; }


    protected Vector2 beforePauseVelocity;

    // A bullet can only hit the same mob once
    protected HashSet<Node2D> mobsHit;

    public Mob shardParent;

    public int numHit { get; private set; }
    public List<VisualEffect> visualEffects { get; set; }

    public Dictionary<StaticColourChange, float> staticColours { get; set; }
    public Dictionary<ParticleEffect, GpuParticles2D> instantiatedParticles { get; set; } // Particle effects should only instantiate once

    public HashSet<Improvement> overwrittenSources { get; set; }

    public List<Node2D> attachedParticleNodes;
    // true only if this was sent by the player (not by another bullet)


    float shardMult = 1;


    float scaleMult = 1;

    Vector2 initialVelocity;


    // Called when the node enters the scene tree for the first time.
    public virtual void Initialize()
    {
        dead = false;

        shardMult = isShard ? Unlocks.splinterDamageMultiplier.GetDynamicVal() : 1;
        SetDamage(Damage.GetDynamicVal() * shardMult);
        AssignDamageType();
        SetSeed(GD.Randf());

        numHit = 0;
        if (mobsHit is null)
        {
            mobsHit = new HashSet<Node2D>();

        }


        lifetime = 0;

        // GD.Print("wallbounce");

        // if (this is not LaserBeam)
        // {
        //     SetScale();
        // }

        if (Unlocks.Flamethrower.unlocked)
        {
            AddMutation(Mutations.GrowingBullet);
        }

        ActivateInitialMutationEffects();
        GenerateVisualEffects();
        SetScale();
        ((IAffectedByVisualEffects)this).ImmediateVisualEffects();



    }

    protected virtual void AssignDamageType() // Default damage type
    {
        damageType = DamageTypes.Blunt;
    }

    public void SetScale(float mult = 1)
    {
        if (mult != 1) { scaleMult = mult; }
        Vector2 scale = Vector2.One * GetScale();
        foreach (GpuParticles2D p in instantiatedParticles.Values)
        {
            p.Scale = scale;
        }
    }

    public float GetScale()
    {
        return BulletSize.GetDynamicVal() * (isShard ? Unlocks.splinterDamageMultiplier.GetDynamicVal() : 1) * scaleMult;
    }

   

    private void GenerateVisualEffects()
    {
        if (instantiatedParticles is null)
        {
            instantiatedParticles = new();
        }
        foreach (Unlockable u in Unlocks.allUnlockables.Where(u => u.unlocked))
        {
            foreach (VisualEffect visEffect in u.GetVisualEffects())
            {
                ((IAffectedByVisualEffects)this).AddVisualEffect(visEffect);

            }
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
        return lifetime;
    }



    public void AddToHitMobs(Mob mob)
    {
        if (mobsHit is null)
        {
            mobsHit = new HashSet<Node2D>();
        }
        mobsHit.Add(mob);
    }

    public virtual Vector2 GetCurrentVelocity()
    {
        return direction * speed;
    }
    // Each projectile type implements more, this is just something they all need as well
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
    protected virtual void Pause()
    {
        // TODO
    }
    protected virtual void UnPause()
    {
        // TODO
    }

    public virtual void Process(double delta)
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
        double time = Time.GetUnixTimeFromSystem();

        lifetime += (float)delta * (Unlocks.Flamethrower.unlocked ? 3 : 1);

        foreach (Mutation m in GetMutations())
        {
            m.OngoingEffect(delta, this);
        }

        if (lifetime >= Lifetime.GetDynamicVal())
        {
            HandleDeath(null, false);
        }


        ((IAffectedByVisualEffects)this).ProcessVisualEffects((float)delta);

        // if (sprite is not null && timeAlive > 0.5f * Lifetime.GetDynamicVal())
        // {
        //     Color currentColour = sprite.Modulate;
        //     sprite.Modulate = new Color(currentColour.R, currentColour.G, currentColour.B, Mathf.Lerp(1, 0.5f, 2*(Lifetime.GetDynamicVal() -)));
        // }
        time -= Time.GetUnixTimeFromSystem();
        time *= -1;
        if (time > 0.001)
        {
            GD.Print(string.Format("{0} bullet process time: {1}ms", name, time * 1000));
        }
    }

    protected void ActivateInitialMutationEffects()
    {
        foreach (Mutation m in GetMutations())
        {

            m.ImmediateEffect(this);
        }
    }

    protected List<Mutation> GetMutations()
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
    }


    // The bullet's damage can change after certain events
    public void SetDamage(float newDmg)
    {

        dmg = newDmg;
    }

    public void OnCollision(Node2D body)
    {
        // This always happens when we hit something, regardless of if it is the final hit
        HandleCollision(body);
        //  Now do checks for things that are only on final or non-final hit
        if (
            ((numHit <= Unlocks.bouncingBulletBounces.GetDynamicVal()) || // Don't die if we have bounces left
            (lifetime < Unlocks.piercingBulletsPiercingTime.GetDynamicVal()) || // Don't die if we have piercing left
            Unlocks.OverflowBullets.unlocked || Unlocks.Flamethrower.unlocked) && // Don't die if overflow bullets are unlocked
            dmg > 0 // Do die if we've run out of damage, even if we have bounces/pierces left
         )
        {
            // Not the final hit - do stuff that only triggers on intermediate hits
            // GD.Print("intermediate");
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
                lifetime = Unlocks.piercingBulletsPiercingTime.GetDynamicVal();
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
                Unlocks.SpawnLightning(dmg, hitMob, 0);
            }
            if (Unlocks.OverflowBullets.unlocked)
            {
                SetDamage(dmg - Unlocks.overflowLoss.GetDynamicVal() * hitMob.GetHP());
            }
            if (Unlocks.Splinter.unlocked && Unlocks.Laser.unlocked)
            {
                Splinter(hitNode);
            }
            if (Unlocks.BouncingBullets.unlocked)
            {
                Bounce(hitNode);
            }
        }

        foreach (Mutation m in GetMutations())
        {
            m.OnCollision(this);
        }
        if (dmg <= 0)
        {
            return;
        }
        // Finally, actually deal damage
        if (hitNode.IsInGroup("mobs") && !mobsHit.Contains(hitNode))
        {
            numHit++;
            mobsHit.Add(hitNode);
            Mob mobHit = (Mob)hitNode;
            mobHit.TakeDamage(dmg, damageType);
        }



    }

    void Bounce(Node2D hitNode)
    {
        Vector2 normal;
        if (hitNode is Mob)
        {
            // Mobs are modelled as spheres that we hit the edge of
            normal = position - hitNode.GlobalPosition;
        }
        else
        {
            // We hit a wall
            normal = hitNode.GetViewportRect().GetCenter() - hitNode.Position;
        }
        normal = normal.Normalized();
        Vector2 newDir = direction - 2 * (direction.Dot(normal)) * normal;
        direction = newDir;
    }


    public virtual void HandleDeath(Node2D lastHit = null, bool splinterOveride = true)
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
        State.bulletManager.DestroyBullet(this);

    }

    bool IsOnScreen()
    {
        Vector2 screenSize = State.bulletManager.GetViewportRect().Size;
        return position.X >= -10 && position.X <= screenSize.X + 10 && position.Y >= -10 && position.Y <= screenSize.Y + 10;
    }

    protected void Splinter(Node2D lastHit = null, int shardCountOveride = -1)
    {
        if (!IsOnScreen())
        {
            return;
        }
        Mob hitMob = null;
        if (lastHit is not null && lastHit is Mob)
        {
            hitMob = (Mob)lastHit;
        }

        int shards = (int)Unlocks.splinterFragments.GetDynamicVal();
        if (shardCountOveride != -1)
        {
            shards = shardCountOveride;
        }
        for (int i = 0; i < shards; i++)
        {
            // Copy myself

            Bullet shard;

            // if (isLaser)
            // {
            //     // If splinter is with laser beam, spawns a laser beam off a hit enemy
                
            //     shard = State.bulletManager.SpawnOrMoveLaser(direction.Rotated(GD.Randf() * 2 * Mathf.Pi), position, true);
            // }
            // else
            // {

                shard = State.bulletManager.SpawnBullet(direction.Rotated((GD.Randf() + 1 / 6) * 1.5f * Mathf.Pi), position, speed, true);

            // }
            foreach (Mutation m in GetMutations())
            {
                shard.AddMutation(m);
            }
            if (hitMob is not null)
            {
                shard.AddToHitMobs(hitMob);
                shard.shardParent = hitMob;
            }
        }
    }



}
