public sealed class EnemyDeadState : EnemyState
{
    public EnemyDeadState(EnemyStateDefinition definition) : base(definition)
    {
    }

    protected override void OnUpdate()
    {
        if (Definition.Animation == null ||
            Renderer.IsAnimationFinished(Definition.Animation))
        {
            Agent.NotifyDeathCompleted();
        }
    }
}
