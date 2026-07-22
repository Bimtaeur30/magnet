using GGMLib.FSM;
using GGMLib.ModuleSystem;
using UnityEngine;

public sealed class EnemyStateMachineModule : MonoBehaviour, IModule
{
    private StateMachine<EnemyAgent> _stateMachine;

    public IState<EnemyAgent> CurrentState => _stateMachine?.CurrentState;

    public void Initialize(ModuleOwner owner)
    {
        EnemyAgent enemyAgent = owner as EnemyAgent;
        Debug.Assert(enemyAgent != null, "EnemyStateMachine은 EnemyAgent의 하위 모듈이어야 합니다.");

        if (enemyAgent != null)
            _stateMachine = new StateMachine<EnemyAgent>(enemyAgent);
    }

    private void Update()
    {
        _stateMachine?.Update();
    }

    private void OnDestroy()
    {
        _stateMachine?.Clear();
    }

    public void ChangeState(IState<EnemyAgent> nextState)
    {
        _stateMachine.ChangeState(nextState);
    }
}
