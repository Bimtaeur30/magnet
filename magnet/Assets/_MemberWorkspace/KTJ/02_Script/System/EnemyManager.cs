using Assets._MemberWorkspace.KTJ._02_Script.Agent.Enemy;
using Game.UI;
using GameLib.EventChannelSystem;
using UnityEngine;

public sealed class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyDataContainerSO EnemyDataContainer;
    [SerializeField] private Transform EnemySpawnPos;
    [SerializeField] private ScoreUIView ScoreUIView;
    [SerializeField] private EventChannelSO EnemyChannel;

    private int _currentStageIdx;
    private EnemyAgent _currentEnemy;
    private IEnemyLifetime _currentEnemyLifetime;

    private void Start()
    {
        Debug.Assert(EnemyDataContainer != null, "EnemyManager의 EnemyDataContainer를 할당하세요.", this);
        Debug.Assert(EnemySpawnPos != null, "EnemyManager의 EnemySpawnPos를 할당하세요.", this);

        SpawnNextEnemy();
    }

    private void SpawnNextEnemy()
    {
        if (EnemyDataContainer == null || EnemySpawnPos == null)
            return;

        EnemyDataSO[] enemies = EnemyDataContainer.EnemyDataContainer;

        if (enemies == null || _currentStageIdx >= enemies.Length)
        {
            OnAllEnemiesDefeated();
            return;
        }

        int spawnIndex = _currentStageIdx;
        EnemyDataSO data = enemies[spawnIndex];
        _currentStageIdx++;

        if (data == null || data.EnemyPrefab == null)
        {
            Debug.LogError($"Enemy data at index {spawnIndex} is invalid.", this);
            SpawnNextEnemy();
            return;
        }

        _currentEnemy = Instantiate(data.EnemyPrefab, EnemySpawnPos);
        _currentEnemy.transform.localPosition = Vector3.zero;

        _currentEnemyLifetime = _currentEnemy;
        _currentEnemyLifetime.Died += OnCurrentEnemyDied;

        _currentEnemy.InitializeEnemyData(data);

        EnemyChannel.RaiseEvent(EnemyEvents.StageClearEvent.Init(_currentStageIdx));
    }

    private void OnCurrentEnemyDied()
    {
        UnsubscribeCurrentEnemy();

        EnemyAgent defeatedEnemy = _currentEnemy;
        _currentEnemy = null;

        if (defeatedEnemy != null)
            Destroy(defeatedEnemy.gameObject);

        SpawnNextEnemy();
    }

    private void OnDestroy()
    {
        UnsubscribeCurrentEnemy();
    }

    private void UnsubscribeCurrentEnemy()
    {
        if (_currentEnemyLifetime == null)
            return;

        _currentEnemyLifetime.Died -= OnCurrentEnemyDied;
        _currentEnemyLifetime = null;
    }

    private void OnAllEnemiesDefeated()
    {
        Debug.Log("All enemies defeated.", this);
    }
}
