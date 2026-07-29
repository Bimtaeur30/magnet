using GGMLib.ModuleSystem;
using System;
using UnityEngine;

namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{
    public class HealthModule : MonoBehaviour, IModule, IHealthModule
    {
        public event Action HealthDepleted;

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }

        public void Initialize(ModuleOwner owner) { }

        public void InitializeData(EnemyDataSO enemyDataSO)
        {
            MaxHealth = enemyDataSO.MaxHealth;
            CurrentHealth = enemyDataSO.MaxHealth;
        }

        public void Damage(int damage)
        {
            if (CurrentHealth <= 0)
                return;

            CurrentHealth -= damage;

            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                HealthDepleted?.Invoke();
            }
        }

        public void Heal(int amount)
        {
            CurrentHealth += amount;

            if (CurrentHealth > MaxHealth)
                CurrentHealth = MaxHealth;
        }
    }
}
