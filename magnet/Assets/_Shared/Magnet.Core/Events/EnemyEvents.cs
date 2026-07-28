using GameLib.EventChannelSystem;
using UnityEngine;

public static class EnemyEvents
{
    public static readonly EnemyAttackEvent EnemyAttackEvent = new();
}

public sealed class EnemyAttackEvent : GameEvent
{
    public Vector3 AttackStartWorldPosition { get; private set; }
    public int Damage { get; private set; }

    public EnemyAttackEvent Init(Vector3 attackStartWorldPosition, int damage)
    {
        AttackStartWorldPosition = attackStartWorldPosition;
        Damage = damage;
        return this;
    }
}
