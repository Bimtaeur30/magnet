public sealed class EnemyDeadState : EnemyState
{
    private bool _deathEffectStarted;

    public EnemyDeadState(EnemyStateDefinition definition) : base(definition)
    {
    }

    protected override void OnEnter()
    {
        _deathEffectStarted = false;
    }

    protected override void OnUpdate()
    {
        if (!_deathEffectStarted)
        {
            if (Definition.Animation != null &&
                !Renderer.IsAnimationFinished(Definition.Animation))
            {
                return;
            }

            _deathEffectStarted = true;
            Renderer.PlayDeathEffect();
            return;
        }

        if (Renderer.IsDeathEffectFinished)
            Agent.NotifyDeathCompleted();
    }
}
