using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Lib/EnemyData")]
    public class EnemyDataSO : ScriptableObject
    {
        public string EnemyName { get; private set; }
        public int MaxHealth { get; private set; }
        public EnemyAgent EnemyPrefab { get; private set; }
    }
}
