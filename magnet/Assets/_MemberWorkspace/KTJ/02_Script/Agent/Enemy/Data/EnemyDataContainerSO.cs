using UnityEngine;

namespace Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy
{

    [CreateAssetMenu(fileName = "EnemyDataContainer", menuName = "Lib/Enemy/EnemyDataContainer")]
    public class EnemyDataContainerSO : ScriptableObject
    {
        [field: SerializeField] public EnemyDataSO[] EnemyDataContainer { get; private set; } // 에너미가 등장하는 순서대로 추가해야함. 이 순서대로 No.n 번호를 부여함.

        private void OnValidate()
        {
            for(int i = 0; i < EnemyDataContainer.Length; i++)
            {
                if (EnemyDataContainer[i] == null) continue;
                EnemyDataSO enemyDataSO = EnemyDataContainer[i];
                enemyDataSO.Initialize(i + 1);
            }
        }
    }
}
