using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Lib/Enemy/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        [field: SerializeField] public EnemyAgent EnemyPrefab { get; private set; }
        [field: SerializeField] public string EnemyName { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }
        [field: SerializeField] public int NoNumber { get; private set; }

        public void Initialize(int noNumber)
        {
            NoNumber = noNumber;
        }
    }
}
