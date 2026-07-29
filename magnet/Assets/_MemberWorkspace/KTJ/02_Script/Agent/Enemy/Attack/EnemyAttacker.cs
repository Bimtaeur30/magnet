using GameLib.EventChannelSystem;
using UnityEngine;

public sealed class EnemyAttacker : MonoBehaviour
{
    [SerializeField] private EventChannelSO enemyEventChannel;
    [SerializeField] private AttackBall attackBallPrefab;
    [SerializeField] private Transform enemyPosition;

    private void Awake()
    {
        Debug.Assert(enemyEventChannel != null, "EnemyAttacker의 Enemy Event Channel을 할당하세요.", this);
        Debug.Assert(attackBallPrefab != null, "EnemyAttacker의 Attack Ball Prefab을 할당하세요.", this);

        enemyEventChannel?.AddListener<EnemyAttackRequestEvent>(OnAttackRequested);
    }

    private void OnDestroy()
    {
        enemyEventChannel?.RemoveListener<EnemyAttackRequestEvent>(OnAttackRequested);
    }

    private void OnAttackRequested(EnemyAttackRequestEvent attackRequest)
    {
        if (attackBallPrefab == null)
            return;

        AttackBall attackBall = Instantiate(
            attackBallPrefab,
            attackRequest.AttackStartWorldPosition,
            Quaternion.identity);

        attackBall.Initialize(
            enemyEventChannel,
            attackRequest.AttackStartWorldPosition,
            enemyPosition.position,
            attackRequest.Damage);
    }
}
