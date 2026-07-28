using GGMLib.ModuleSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Codice.Client.Common.WebApi.WebApiEndpoints;

namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{
    public class HealthModule : MonoBehaviour, IModule, IHealthModule
    {
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }

        private IStateMachineModule _stateMachineModule;

        public void Initialize(ModuleOwner owner)
        {
            _stateMachineModule = owner.GetModule<IStateMachineModule>();
        }

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
                _stateMachineModule.ChangeState(EnemyStateId.Dead);
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
