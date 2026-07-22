public sealed class EnemySpawnState : EnemyState
{
    public EnemySpawnState(EnemyStateDefinition definition) : base(definition)
    {
    }

    protected override void OnUpdate()
    {
        if (Definition.HasNextState && Renderer.IsAnimationFinished(Definition.Animation))
            Agent.GetModule<EnemyStateMachineModule>().ChangeState(Definition.NextState);
    }
}
