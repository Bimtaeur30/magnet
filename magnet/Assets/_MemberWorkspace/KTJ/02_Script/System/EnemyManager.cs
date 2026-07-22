using Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyDataContainerSO EnemyDataContainer;
    [SerializeField] private Transform EnemySpawnPos;
    private int _currentStageIdx = 0;

    private void Start()
    {
        SpawnEnemy(_currentStageIdx);
    }

    private void SpawnEnemy(int idx)
    {
        EnemyDataSO data = EnemyDataContainer.EnemyDataContainer[idx];
        EnemyAgent enemy = Instantiate(data.EnemyPrefab, EnemySpawnPos);
        enemy.gameObject.transform.localPosition = Vector3.zero;
        enemy.InitializeEnemyData(data);
    }
}
