using GameLib.EventChannelSystem;
using UnityEngine;

public static class EnemyEvents
{
    public static readonly EnemyAttackRequestEvent EnemyAttackRequestEvent = new();
    public static readonly EnemyAttackEvent EnemyAttackEvent = new();
    public static readonly StageClearEvent StageClearEvent = new();
}

public sealed class StageClearEvent : GameEvent
{
    public int ClearStageIdx;

    public StageClearEvent Init(int clearStageIdx)
    {
        ClearStageIdx =  clearStageIdx;
        return this;
    }
}
public sealed class EnemyAttackRequestEvent : GameEvent
{
    public Vector3 AttackStartWorldPosition { get; private set; }
    public float Damage { get; private set; }

    public EnemyAttackRequestEvent Init(
        Vector3 attackStartWorldPosition,
        float damage)
    {
        AttackStartWorldPosition = attackStartWorldPosition;
        Damage = damage;
        return this;
    }
}

public sealed class EnemyAttackEvent : GameEvent
{
    public Vector3 AttackEndWorldPosition { get; private set; }
    public float Damage { get; private set; }

    public EnemyAttackEvent Init(Vector3 attackEndWorldPosition, float damage)
    {
        AttackEndWorldPosition = attackEndWorldPosition;
        Damage = damage;
        return this;
    }
}
