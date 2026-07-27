using GGMLib.FSM;

public abstract class EnemyState : IState<EnemyAgent>
{
    protected EnemyAgent Agent { get; private set; }
    protected IAgentRenderer Renderer { get; private set; }
    protected EnemyStateDefinition Definition { get; }

    protected EnemyState(EnemyStateDefinition definition)
    {
        Definition = definition;
    }

    public void Enter(EnemyAgent context)
    {
        Agent = context;
        Renderer = Agent.GetModule<IAgentRenderer>();

        if (Definition.Animation != null)
            Renderer.PlayAnimation(Definition.Animation);

        OnEnter();
    }

    public void Update(EnemyAgent context)
    {
        OnUpdate();
    }

    public void Exit(EnemyAgent context)
    {
        OnExit();
        Renderer = null;
        Agent = null;
    }

    protected virtual void OnEnter() { }
    protected virtual void OnUpdate() { }
    protected virtual void OnExit() { }
}
