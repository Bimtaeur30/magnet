using System;
using System.Collections.Generic;
using GGMLib.FSM;
using GGMLib.ModuleSystem;
using UnityEngine;

public sealed class EnemyStateMachineModule : MonoBehaviour, IModule, IStateMachineModule
{
    [SerializeField] private EnemyStateConfigSO stateConfig;

    private readonly Dictionary<EnemyStateId, EnemyState> _states = new();
    private StateMachine<EnemyAgent> _stateMachine;

    public IState<EnemyAgent> CurrentState => _stateMachine?.CurrentState;

    public void Initialize(ModuleOwner owner)
    {
        EnemyAgent enemyAgent = owner as EnemyAgent;
        Debug.Assert(enemyAgent != null, "EnemyStateMachine은 EnemyAgent의 하위 모듈이어야 합니다.");

        if (enemyAgent == null)
            return;

        _stateMachine = new StateMachine<EnemyAgent>(enemyAgent);
        CreateStates();
    }

    private void CreateStates()
    {
        _states.Clear();

        if (stateConfig == null)
        {
            Debug.LogError("EnemyStateConfigSO가 할당되지 않았습니다.", this);
            return;
        }

        foreach (EnemyStateDefinition definition in stateConfig.States)
        {
            if (_states.ContainsKey(definition.StateId))
            {
                Debug.LogError($"{definition.StateId} 상태가 중복 등록되었습니다.", stateConfig);
                continue;
            }

            _states.Add(definition.StateId, CreateState(definition));
        }
    }

    private static EnemyState CreateState(EnemyStateDefinition definition)
    {
        return definition.StateId switch
        {
            EnemyStateId.Spawn => new EnemySpawnState(definition),
            EnemyStateId.Idle => new EnemyIdleState(definition),
            EnemyStateId.Damage => new EnemyDamageState(definition),
            EnemyStateId.Dead => new EnemyDeadState(definition),
            _ => throw new ArgumentOutOfRangeException(nameof(definition.StateId), definition.StateId, null)
        };
    }

    private void Update()
    {
        _stateMachine?.Update();
    }

    private void OnDestroy()
    {
        _stateMachine?.Clear();
    }

    public void StartStateMachine()
    {
        ChangeState(stateConfig.InitialState);
    }

    public void ChangeState(EnemyStateId stateId)
    {
        if (!_states.TryGetValue(stateId, out EnemyState state))
        {
            Debug.LogError($"{stateId} 상태가 등록되지 않았습니다.", this);
            return;
        }

        _stateMachine.ChangeState(state);
    }
}
