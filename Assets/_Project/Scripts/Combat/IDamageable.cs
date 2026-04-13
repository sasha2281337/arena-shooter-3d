namespace ArenaShooter.Combat
{
    public interface IDamageable
    {
        bool IsAlive { get; }
        void TakeDamage(float damage);
    }
}
