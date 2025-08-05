using Godot;
using static Stats.EnemyStats;





public abstract partial class Organic : Mob
{
    // Things that drop money when they die

    
    protected override void Die()
    {
        Stats.Counters.KillCounter.Value++;
        float tempDropRate = Stats.PlayerStats.DropRate.GetDynamicVal();
        // If drop rate is above 1, get 1 guaranteed coin plus a chance at another
        while (tempDropRate > 0)
        {
            if (GD.RandRange(0f, 1) <= tempDropRate)
            {
                SpawnCoin();
            }
            tempDropRate--;
        }
        if (explodeOnDeath)
        {

            Explode();

        }
        base.Die();
    }
}