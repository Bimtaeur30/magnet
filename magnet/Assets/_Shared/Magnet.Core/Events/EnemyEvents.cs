using GameLib.EventChannelSystem;
using UnityEngine;

public static class EnemyEvents
{
    public static readonly EnemyAttackRequestEvent EnemyAttackRequestEvent = new();
    public static readonly EnemyAttackEvent EnemyAttackEvent = new();
}

public sealed class EnemyAttackRequestEvent : GameEvent
{
    public Vector3 AttackStartWorldPosition { get; private set; }
    public Vector3 AttackEndWorldPosition { get; private set; }
    public int Damage { get; private set; }

    public EnemyAttackRequestEvent Init(
        Vector3 attackStartWorldPosition,
        Vector3 attackEndWorldPosition,
        int damage)
    {
        AttackStartWorldPosition = attackStartWorldPosition;
        AttackEndWorldPosition = attackEndWorldPosition;
        Damage = damage;
        return this;
    }
}

public sealed class EnemyAttackEvent : GameEvent
{
    public Vector3 AttackEndWorldPosition { get; private set; }
    public int Damage { get; private set; }

    public EnemyAttackEvent Init(Vector3 attackEndWorldPosition, int damage)
    {
        AttackEndWorldPosition = attackEndWorldPosition;
        Damage = damage;
        return this;
    }
}
