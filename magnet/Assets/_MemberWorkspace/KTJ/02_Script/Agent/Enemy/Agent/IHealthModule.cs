using System;

namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{
    public interface IHealthModule
    {
        event Action HealthDepleted;

        float CurrentHealth { get; }

        void InitializeData(EnemyDataSO enemyDataSO);
        void Damage(float damage);
        void Heal(float amount);
    }
}
