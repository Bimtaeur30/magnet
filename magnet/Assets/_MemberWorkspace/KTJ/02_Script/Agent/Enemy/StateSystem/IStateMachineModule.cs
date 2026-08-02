public interface IStateMachineModule
{
    void ChangeState(EnemyStateId stateId);
    void StartStateMachine();
}