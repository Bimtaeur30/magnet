using System;
using GGMLib.Anim;
using UnityEngine;

[Serializable]
public sealed class EnemyStateDefinition
{
    [field: SerializeField] public EnemyStateId StateId { get; private set; }
    [field: SerializeField] public AnimationParamSO Animation { get; private set; }
    [field: SerializeField] public bool HasNextState { get; private set; }
    [field: SerializeField] public EnemyStateId NextState { get; private set; }
}
