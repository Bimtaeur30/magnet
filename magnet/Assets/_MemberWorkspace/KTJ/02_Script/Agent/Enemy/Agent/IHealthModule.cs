namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{
    public interface IHealthModule
    {
        int CurrentHealth { get; }

        void InitializeData(EnemyDataSO enemyDataSO);
        void Damage(int damage);
        void Heal(int amount);
    }
}
