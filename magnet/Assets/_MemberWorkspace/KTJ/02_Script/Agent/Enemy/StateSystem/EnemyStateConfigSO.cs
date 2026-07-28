using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy State Config", menuName = "Lib/Enemy/State Config")]
public sealed class EnemyStateConfigSO : ScriptableObject
{
    [field: SerializeField] public EnemyStateId InitialState { get; private set; } = EnemyStateId.Spawn;
    [field: SerializeField] public List<EnemyStateDefinition> States { get; private set; } = new();
}
